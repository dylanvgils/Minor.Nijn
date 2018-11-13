namespace Minor.Nijn
{
    public class CommandMessage
    {
        public string Message { get; }
        public string Type { get; }
        public string CorrelationId { get; }

        public CommandMessage(string message, string type, string correlationId)
        {
            Message = message;
            Type = type;
            CorrelationId = correlationId;
        }
    }
}
