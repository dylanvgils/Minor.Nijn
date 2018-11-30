using System;

namespace Minor.Nijn.WebScale.Events
{
    public abstract class DomainEvent
    {
        public string RoutingKey { get; }
        public long Timestamp { get; }
        public string CorrelationId { get; }

        protected DomainEvent(string routingKey)
        {
            RoutingKey = routingKey;
            Timestamp = DateTime.Now.Ticks;
            CorrelationId = Guid.NewGuid().ToString();
        }
    }
}