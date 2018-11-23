using Minor.Nijn.WebScale.Events;

namespace Minor.Nijn.WebScale.Test.TestClasses.Events
{
    public class ProductAddedEvent : DomainEvent
    {
        public ProductAddedEvent(string routingKey) : base(routingKey)
        {
        }
    }
}
