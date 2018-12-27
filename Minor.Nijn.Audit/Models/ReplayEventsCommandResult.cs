namespace Minor.Nijn.Audit.Models
{
    public class ReplayEventsCommandResult
    {
        public string ExchangeName { get; set; }
        public long StartTimestamp { get; set; }
        public int NumberOfEvents { get; set; }
    }
}