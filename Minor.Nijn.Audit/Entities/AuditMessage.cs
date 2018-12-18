namespace Minor.Nijn.Audit.Entities
{
    public class AuditMessage
    {
        public long Id { get; set; }
        public string RoutingKey { get; set; }
        public long Timestamp { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
    }
}