using System;
using Newtonsoft.Json;

namespace Minor.Nijn.WebScale.Events
{
    public abstract class DomainEvent
    {
        [JsonProperty]
        public string RoutingKey { get; }

        [JsonProperty]
        public long Timestamp { get; internal set; }

        [JsonProperty]
        public string CorrelationId { get; internal set; }

        protected DomainEvent(string routingKey)
        {
            RoutingKey = routingKey;
            Timestamp = DateTime.Now.Ticks;
            CorrelationId = Guid.NewGuid().ToString();
        }
    }
}