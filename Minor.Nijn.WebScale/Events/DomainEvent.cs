using Newtonsoft.Json;
using System;

namespace Minor.Nijn.WebScale.Events
{
    /// <summary>
    /// Base class for all domain events
    /// </summary>
    public abstract class DomainEvent
    {
        /// <summary>
        /// The Routing Key is used by the underlying protocol to route events to subscribers
        /// </summary>
        [JsonProperty]
        public string RoutingKey { get; }

        /// <summary>
        /// The Timestamp is set to the creation time of the domain event.
        /// </summary>
        [JsonProperty]
        public long Timestamp { get; internal set; }

        /// <summary>
        /// The CorrelationId uniquely identifies the domain event.
        /// </summary>
        [JsonProperty]
        public string CorrelationId { get; internal set; }

        /// <summary>
        /// Creates a domain event by setting the routing key and
        /// generating a timestamp and correlationId
        /// </summary>
        /// <param name="routingKey"></param>
        protected DomainEvent(string routingKey)
        {
            RoutingKey = routingKey;
            Timestamp = DateTime.Now.Ticks;
            CorrelationId = Guid.NewGuid().ToString();
        }
    }
}