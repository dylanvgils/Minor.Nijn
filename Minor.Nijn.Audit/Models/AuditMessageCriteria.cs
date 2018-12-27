namespace Minor.Nijn.Audit.Models
{
    public class AuditMessageCriteria
    {
        public long? FromTimestamp { get; set; }
        public long? ToTimestamp { get; set; }
        public string EventType { get; set; }
        public string RoutingKeyExpression { get; set; }
    }
}