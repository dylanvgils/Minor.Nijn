using System.Collections.Generic;

namespace Minor.Nijn.TestBus.EventBus
{
    internal class EventBusQueue : TestBusQueue<EventMessage> 
    {
        public IEnumerable<string> TopicExpressions { get; }

        public EventBusQueue(string name, IEnumerable<string> topicExpressions) : base(name)
        {
            TopicExpressions = topicExpressions;
        }

        public override void Enqueue(EventMessage message)
        {
            if (TopicMatcher.IsMatch(TopicExpressions, message.RoutingKey))
            {
                base.Enqueue(message);
            }
        }
    }
}