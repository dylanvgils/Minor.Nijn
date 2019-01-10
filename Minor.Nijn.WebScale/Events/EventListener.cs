using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Minor.Nijn.WebScale.Events
{
    internal class EventListener : IEventListener
    {
        private readonly ILogger _logger;
        private long _listeningFromTimestamp;

        public EventListenerInfo Meta { get; }
        public string QueueName => Meta.QueueName;
        public IEnumerable<string> TopicExpressions => Meta.Methods.SelectMany(m => m.TopicExpressions);

        private object _instance;

        private IMessageReceiver _receiver;
        private IMicroserviceHost _host;
        private bool _isDeclared;
        private bool _isListening;
        private bool _disposed;

        public EventListener(EventListenerInfo meta)
        {
            Meta = meta;
            _logger = NijnWebScaleLogger.CreateLogger<EventListener>();
        }

        public void RegisterListener(IMicroserviceHost host)
        {
            CheckDisposed();
            if (_isDeclared)
            {
                _logger.LogError("Event listener already registered");
                throw new InvalidOperationException("Event listener already registered");
            }

            _host = host;
            _receiver = host.Context.CreateMessageReceiver(QueueName, TopicExpressions);
            _receiver.DeclareQueue();

            _isDeclared = true;
        }

        public void StartListening(long fromTimestamp)
        {
            CheckDisposed();
            if (!_isDeclared)
            {
                _logger.LogError("Event listener is not declared");
                throw new InvalidOperationException("Event listener is not declared");
            }

            if (_isListening)
            {
                _logger.LogError("Event listener already listening");
                throw new InvalidOperationException("Event listener already listening");
            }

            _instance = Meta.IsSingleton ? _host.CreateInstance(Meta.Type) : null;
            _receiver.StartReceivingMessages(HandleEventMessage);

            _listeningFromTimestamp = fromTimestamp;
            _isListening = true;
        }

        public void HandleEventMessage(EventMessage message)
        {
            if (message.Timestamp < _listeningFromTimestamp)
            {
                _logger.LogInformation("Skip EventMessage with correlationId: {0}, timestamp was before provided timestamp.", message.CorrelationId);
                return;
            }

            var instance = Meta.IsSingleton ? _instance : _host.CreateInstance(Meta.Type);
            var methods = Meta.Methods.Where(m => TopicMatcher.IsMatch(m.TopicExpressions, message.RoutingKey));

            foreach (var method in methods)
            {
                var isEventMessage = method.EventType == typeof(EventMessage);
                if (!isEventMessage && message.Type != method.EventType.Name)
                {
                    _logger.LogError(
                        "Received message in invalid format, expected message to be of type {0} and got {1}",
                        method.EventType.Name, message.Type);
                    return;
                }

                object payload = message;
                if (!isEventMessage)
                {
                    payload = JsonConvert.DeserializeObject(message.Message, method.EventType);

                    // TODO: Set these properties through the JSON Deserializer
                    payload.GetType().GetProperty("CorrelationId").SetValue(payload, message.CorrelationId);
                    payload.GetType().GetProperty("Timestamp").SetValue(payload, message.Timestamp);
                }

                InvokeListener(instance, method, payload);
            }
        }

        private static void InvokeListener(object instance, EventListenerMethodInfo methodInfo, params object[] payload)
        {
            if (methodInfo.IsAsync)
            {
                var task = (Task)methodInfo.Method.Invoke(instance, payload);
                task.Wait();
                return;
            }

            methodInfo.Method.Invoke(instance, payload);
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EventListener()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _receiver?.Dispose();
            }

            _disposed = true;
        }
    }
}
