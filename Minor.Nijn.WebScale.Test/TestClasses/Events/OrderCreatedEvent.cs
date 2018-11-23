using Minor.Nijn.WebScale.Events;
using Minor.Nijn.WebScale.Test.TestClasses.Domain;

namespace Minor.Nijn.WebScale.Test.TestClasses.Events
{
    public class OrderCreatedEvent : DomainEvent
    {
        public Order Order { get;  }

        public OrderCreatedEvent(string routingKey, Order order) : base(routingKey)
        {
            Order = order;
        }
    }
}