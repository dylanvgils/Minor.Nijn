using System;

namespace Minor.Nijn
{
    public abstract class CommandMessage
    {
        public string RoutingKey { get; protected set; }
        public string Type { get; }
        public string CorrelationId { get; }
        public string Message { get; }

        protected CommandMessage(string message, string type, string correlationId)
        {
            Message = message;
            Type = type;
            CorrelationId = correlationId;
        }
    }

    public class RequestCommandMessage : CommandMessage
    {
        internal RequestCommandMessage(string message, string type, string correlationId) : base(message, type, correlationId)
        {
        }

        public RequestCommandMessage(string message, string type, string correlationId, string routingKey) : base(message, type, correlationId)
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

        public ResponseCommandMessage(string message, string type, string correlationId) : base(message, type, correlationId)
        {
        }
    }
}
