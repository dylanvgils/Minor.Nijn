using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn
{
    public class EventMessage
    {
        public string RoutingKey { get; }
        public string Message { get; }
        public string EventType { get; }
        public long Timestamp { get; }
        public string CorrelationId { get; }

        public EventMessage(string routingKey, string message, string eventType = null, long timestamp=0, string correlationId=null)
        {
            RoutingKey = routingKey;
            Message = message;
            EventType = eventType;
            Timestamp = timestamp;
            CorrelationId = correlationId;
        }
    }
}
