using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace Minor.Nijn.WebScale
{
    public class EventListener
    {
        public string QueueName { get; }
        public IEnumerable<string> TopicPatterns { get; }

        private readonly MethodInfo _method;
        private readonly object _instance;

        internal EventListener(Type type, MethodInfo method, string queueName, IEnumerable<string> topicPattern)
        {
            QueueName = queueName;
            TopicPatterns = topicPattern;
            _method = method;

            _instance = Activator.CreateInstance(type);
        }

        // TODO: Add parameter check, parameter has to be derived type of DomainEvent
        internal void HandleEventMessage(EventMessage message)
        {
            var paramType = _method.GetParameters()[0].ParameterType;
            var payload = JsonConvert.DeserializeObject(message.Message, paramType);
            _method.Invoke(_instance, new [] { payload });
        }
    }
}
