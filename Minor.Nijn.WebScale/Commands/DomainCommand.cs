using System;

namespace Minor.Nijn.WebScale.Commands
{
    public class DomainCommand
    {
        public string Command { get; }
        public long Timestamp { get; }
        public string CorrelationId { get; }

        public DomainCommand(string command)
        {
            Command = command;
            Timestamp = DateTime.Now.Ticks;
            CorrelationId = Guid.NewGuid().ToString();
        }
    }
}