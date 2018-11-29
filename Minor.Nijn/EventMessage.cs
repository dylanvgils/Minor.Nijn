namespace Minor.Nijn
{
    public class EventMessage
    {
        public string RoutingKey { get; }
        public string Type { get; }
        public long Timestamp { get; }
        public string CorrelationId { get; }
        public string Message { get; }

        public EventMessage(string routingKey, string message, string type = null, long timestamp=0, string correlationId=null)
        {
            RoutingKey = routingKey;
            Message = message;
            Type = type;
            Timestamp = timestamp;
            CorrelationId = correlationId;
        }
    }
}
