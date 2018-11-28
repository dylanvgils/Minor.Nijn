using System;

namespace Minor.Nijn.WebScale.Commands
{
    public abstract class DomainCommand
    {
        public string RoutingKey { get; }
        public long Timestamp { get; }
        public string CorrelationId { get; }

        protected DomainCommand(string routingKey)
        {
            RoutingKey = routingKey;
            Timestamp = DateTime.Now.Ticks;
            CorrelationId = Guid.NewGuid().ToString();
        }
    }
}