using System.Collections.Generic;

namespace Minor.Nijn.TestBus.EventBus
{
    internal sealed class EventBus : BaseBus<EventBusQueue, EventMessage>, IEventBus
    {        
        public EventBusQueue DeclareQueue(string queueName, IEnumerable<string> topicExpressions)
        {
            return base.DeclareQueue(queueName, new EventBusQueue(queueName, topicExpressions));
        }   
    }
}
