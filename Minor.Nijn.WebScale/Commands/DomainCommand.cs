using Newtonsoft.Json;
using System;

namespace Minor.Nijn.WebScale.Commands
{
    /// <summary>
    /// Base class for all domain commands
    /// </summary>
    public abstract class DomainCommand
    {
        /// <summary>
        /// The routing key the command should be send to, in most cases
        /// this is the queue name
        /// </summary> 
        [JsonProperty]
        public string RoutingKey { get; }

        /// <summary>
        /// Unix timestamp representing the creation time of the command
        /// </summary>
        [JsonProperty]
        public long Timestamp { get; internal set; }

        /// <summary>
        /// The CorrelationId uniquely identifies the command
        /// </summary>
        [JsonProperty]
        public string CorrelationId { get; internal set; }

        /// <summary>
        /// Creates a domain command by setting the routing key and
        /// generating a timestamp and correlationId
        /// </summary>
        /// <param name="routingKey"></param>
        protected DomainCommand(string routingKey)
        {
            RoutingKey = routingKey;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            CorrelationId = Guid.NewGuid().ToString();
        }
    }
}