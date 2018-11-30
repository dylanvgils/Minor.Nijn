using System;
using Newtonsoft.Json;

namespace Minor.Nijn.WebScale.Commands
{
    public abstract class DomainCommand
    {
        [JsonProperty]
        public string RoutingKey { get; }

        [JsonProperty]
        public long Timestamp { get; internal set; }

        [JsonProperty]
        public string CorrelationId { get; internal set; }

        protected DomainCommand(string routingKey)
        {
            RoutingKey = routingKey;
            Timestamp = DateTime.Now.Ticks;
            CorrelationId = Guid.NewGuid().ToString();
        }
    }
}