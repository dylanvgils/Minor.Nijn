﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Minor.Nijn.WebScale.Events
{
    public class EventListener : IEventListener
    {
        private readonly ILogger _logger;

        public string QueueName { get; }
        public IEnumerable<string> TopicExpressions { get; }

        public readonly Type _type;
        private readonly MethodInfo _method;
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

        // TODO: Add parameter check, parameter has to be derived type of DomainEvent
        internal void HandleEventMessage(EventMessage message)
        {
            var paramType = _method.GetParameters()[0].ParameterType;
            var payload = JsonConvert.DeserializeObject(message.Message, paramType);
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
