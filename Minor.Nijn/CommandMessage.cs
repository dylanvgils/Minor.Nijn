using System;

namespace Minor.Nijn
{
    public abstract class CommandMessage
    {
        public string RoutingKey { get; protected set; }
        public string Type { get; }
        public long Timestamp { get; }
        public string CorrelationId { get; }
        public string Message { get; }

        protected CommandMessage(string message, string type, string correlationId, long timestamp = 0)
        {
            Message = message;
            Type = type;
            CorrelationId = correlationId;
            Timestamp = timestamp;
        }
    }

    public class RequestCommandMessage : CommandMessage
    {
        internal RequestCommandMessage(string message, string type, string correlationId, long timestamp = 0) 
            : base(message, type, correlationId, timestamp)
        {
        }

        public RequestCommandMessage(string message, string type, string correlationId, string routingKey, long timestamp = 0) 
            : base(message, type, correlationId, timestamp)
        {
            RoutingKey = routingKey;
        }
    }

    public class ResponseCommandMessage : CommandMessage
    {
        public new string RoutingKey
        {
            get => base.RoutingKey;
            set => base.RoutingKey = value;
        }

        public ResponseCommandMessage(string message, string type, string correlationId, long timestamp = 0) 
            : base(message, type, correlationId, timestamp)
        {
        }
    }
}
