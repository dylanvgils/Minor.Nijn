using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Nijn.WebScale.Events
{
    public class EventListener : IEventListener
    {
        private readonly ILogger _logger;

        public string QueueName { get; }
        public IEnumerable<string> TopicExpressions { get; }

        private readonly MethodInfo _method;
        private readonly object _instance;

        private IMessageReceiver _receiver;
        private bool _isListening;

        internal EventListener(Type type, MethodInfo method, string queueName, IEnumerable<string> topicExpressions)
        {
            QueueName = queueName;
            TopicExpressions = topicExpressions;
            _method = method;

            _instance = Activator.CreateInstance(type);
            _logger = NijnWebScaleLogger.CreateLogger<EventListener>();
        }

        public void StartListening(IBusContext<IConnection> context)
        {
            if (_isListening)
            {
                _logger.LogDebug("Event listener already listening for events");
                throw new InvalidOperationException("Already listening for events");
            }

            _receiver = context.CreateMessageReceiver(QueueName, TopicExpressions);
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

        public void Dispose()
        {
            _receiver?.Dispose();
        }
    }
}
