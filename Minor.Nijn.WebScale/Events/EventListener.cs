using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Minor.Nijn.WebScale.Events
{
    internal class EventListener : IEventListener
    {
        private readonly ILogger _logger;

        public string QueueName { get; }
        public IEnumerable<string> TopicExpressions { get; }

        private readonly Type _type;
        private readonly MethodInfo _method;
        private readonly Type _eventType;
        private object _instance;

        private IMessageReceiver _receiver;
        private bool _isListening;
        private bool _disposed;

        internal EventListener(Type type, MethodInfo method, string queueName, IEnumerable<string> topicExpressions)
        {
            QueueName = queueName;
            TopicExpressions = topicExpressions;

            _type = type;
            _method = method;
            _eventType = method.GetParameters()[0].ParameterType;

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

            _instance = host.CreateInstance(_type);

            _receiver = host.Context.CreateMessageReceiver(QueueName, TopicExpressions);
            _receiver.DeclareQueue();
            _receiver.StartReceivingMessages(HandleEventMessage);

            _isListening = true;
        }

        internal void HandleEventMessage(EventMessage message)
        {
            if (message.Type != _eventType.Name)
            {
                _logger.LogError("Received message in invalid format, expected message to be of type {0} and got {1}", 
                    _eventType.Name, message.Type);
                return;
            }

            var payload = JsonConvert.DeserializeObject(message.Message, _eventType);

            // TODO: Set these properties through the JSON Deserializer
            payload.GetType().GetProperty("CorrelationId").SetValue(payload, message.CorrelationId);
            payload.GetType().GetProperty("Timestamp").SetValue(payload, message.Timestamp);

            _method.Invoke(_instance, new [] { payload });
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
