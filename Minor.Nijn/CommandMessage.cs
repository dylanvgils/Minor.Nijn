namespace Minor.Nijn
{
    public class CommandMessage
    {
        public string RoutingKey { get; set; }
        public string Type { get; }
        public string CorrelationId { get; }

        public string Message { get; }

        public CommandMessage(string message, string type, string correlationId)
        {
            Message = message;
            Type = type;
            CorrelationId = correlationId;
        }

        public CommandMessage(string message, string type, string correlationId, string routingKey)
        {
            Message = message;
            Type = type;
            CorrelationId = correlationId;
            RoutingKey = routingKey;
        }
    }
}
