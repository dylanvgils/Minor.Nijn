using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Test.TestClasses.Domain;

namespace Minor.Nijn.WebScale.Test.TestClasses.Commands
{
    public class AddOrderCommand : DomainCommand
    {
        public Order Order { get; }

        public AddOrderCommand(string routingKey, Order order) : base(routingKey) {
            Order = order;
        }
    }
}