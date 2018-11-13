using System.Collections.Generic;

namespace Minor.Nijn.TestBus.EventBus
{
    internal sealed class EventBus : BaseBus<EventBusQueue, EventMessage>, IEventBus
    {
        public EventBusQueue DeclareQueue(string queueName, IEnumerable<string> topicExpressions)
        {
            if (_queues.ContainsKey(queueName))
            {
                return _queues[queueName];
            }
            
            var queue = new EventBusQueue(queueName, topicExpressions);
            _queues.Add(queueName, queue);
            return queue;
        }
    }
}
