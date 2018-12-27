using Minor.Nijn.WebScale.Commands;

namespace Minor.Nijn.Audit.Models
{
    public class ReplayEventsCommand : DomainCommand
    {
        public string ExchangeName { get; set; }
        public long? FromTimestamp { get; set; }
        public long? ToTimestamp { get; set; }
        public string EventType { get; set; }
        public string RoutingKeyExpression { get; set; }

        public ReplayEventsCommand(string routingKey) : base(routingKey)
        {
        }
    }
}