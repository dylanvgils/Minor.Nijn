using Minor.Nijn.WebScale.Events;

namespace ConsoleAppExample.Domain
{
    public class SaidHelloEvent : DomainEvent
    {
        public string Message { get; }

        public SaidHelloEvent(string message, string routingKey) : base(routingKey)
        {
            Message = message;
        }
    }
}
