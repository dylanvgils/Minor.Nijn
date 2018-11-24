using Minor.Nijn.WebScale.Commands;

namespace Minor.Nijn.WebScale.Test.TestClasses.Commands
{
    public class AddProductCommand : DomainCommand
    {
        public AddProductCommand(string routingKey) : base(routingKey) { }
    }
}