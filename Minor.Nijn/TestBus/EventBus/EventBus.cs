using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.TestBus.EventBus
{
    internal sealed class EventBus : IEventBus
    {
        private readonly IDictionary<string, EventBusQueue> _queues;
        public int QueueLength => _queues.Count;

        public EventBus()
        {
            _queues = new Dictionary<string, EventBusQueue>();
        }

        public void DispatchMessage(EventMessage message)
        {
            foreach (var queue in _queues)
            {
                queue.Value.Enqueue(message);
            }
        }

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
