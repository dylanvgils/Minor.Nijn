namespace Minor.Nijn
{
    public class CommandMessage
    {
        internal string ReplyTo { get; set; }
        
        public string Message { get; }
        public string Type { get; }
        public string CorrelationId { get; }

        public CommandMessage(string message, string type, string correlationId)
        {
            Message = message;
            Type = type;
            CorrelationId = correlationId;
        }

        internal CommandMessage(string message, string type, string correlationId, string replyTo)
        {
            Message = message;
            Type = type;
            CorrelationId = correlationId;
            ReplyTo = replyTo;
        }
    }
}
