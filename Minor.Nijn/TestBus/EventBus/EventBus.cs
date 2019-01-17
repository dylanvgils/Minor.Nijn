using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Minor.Nijn.TestBus.EventBus
{
    internal sealed class EventBus : BaseBus<EventBusQueue, EventMessage>, IEventBus
    {        
        public EventBusQueue DeclareQueue(string queueName, IEnumerable<string> topicExpressions)
        {
            if (!Queues.TryGetValue(queueName, out var queue))
            {
                return DeclareQueue(queueName, new EventBusQueue(queueName, topicExpressions));
            }

            var expressions = new List<string>(queue.TopicExpressions);
            expressions.AddRange(topicExpressions);
            queue.TopicExpressions = expressions;

            return queue;
        }   
    }
}
