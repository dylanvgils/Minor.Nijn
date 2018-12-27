using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Minor.Nijn.WebScale.Events
{
    internal class EventListener : IEventListener
    {
        private readonly ILogger _logger;

        public EventListenerInfo Meta { get; }
        public string QueueName => Meta.QueueName;
        public IEnumerable<string> TopicExpressions => Meta.TopicExpressions;

        private IMessageReceiver _receiver;
        private IMicroserviceHost _host;
        private bool _isListening;
        private bool _disposed;

        public EventListener(EventListenerInfo meta)
        {
            Meta = meta;
            _logger = NijnWebScaleLogger.CreateLogger<EventListener>();
        }

        public void StartListening(IMicroserviceHost host)
        {
            CheckDisposed();
            if (_isListening)
            {
                _logger.LogDebug("Event listener already listening for events");
                throw new InvalidOperationException("Already listening for events");
            }

            _host = host;
            _receiver = host.Context.CreateMessageReceiver(QueueName, TopicExpressions);
            _receiver.DeclareQueue();
            _receiver.StartReceivingMessages(HandleEventMessage);

            _isListening = true;
        }

        public void HandleEventMessage(EventMessage message)
        {
            var instance = _host.CreateInstance(Meta.Type);
            var isEventMessage = Meta.EventType == typeof(EventMessage);

            if (!isEventMessage && message.Type != Meta.EventType.Name)
            {
                _logger.LogError("Received message in invalid format, expected message to be of type {0} and got {1}", 
                    Meta.EventType.Name, message.Type);
                return;
            }

            object payload = message;
            if (!isEventMessage)
            {
                payload = JsonConvert.DeserializeObject(message.Message, Meta.EventType);

                // TODO: Set these properties through the JSON Deserializer
                payload.GetType().GetProperty("CorrelationId").SetValue(payload, message.CorrelationId);
                payload.GetType().GetProperty("Timestamp").SetValue(payload, message.Timestamp);
            }

            InvokeListener(instance, payload);
        }

        private void InvokeListener(object instance, params object[] payload)
        {
            if (Meta.IsAsyncMethod)
            {
                var task = (Task) Meta.Method.Invoke(instance, payload);
                task.Wait();
                return;
            }

            Meta.Method.Invoke(instance, payload);
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
