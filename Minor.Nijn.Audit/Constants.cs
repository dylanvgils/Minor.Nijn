namespace Minor.Nijn.Audit
{
    internal static class Constants
    {
        public const string AuditEventQueueName = "Minor.Nijn.Audit.EventQueue";
        public const string AuditEventPattern = "#";

        public const string AuditCommandQueueName = "Minor.Nijn.Audit.CommandQueue";

        public const string ReplayerExchangeType = "topic";
    }
}