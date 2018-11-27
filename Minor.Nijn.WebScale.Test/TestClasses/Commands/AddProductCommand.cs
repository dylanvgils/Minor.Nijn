using Minor.Nijn.WebScale.Commands;

namespace Minor.Nijn.WebScale.Test.TestClasses.Commands
{
    public class AddProductCommand : DomainCommand
    {
        public int Number { get; set; }

        public AddProductCommand(string routingKey, int number) : base(routingKey)
        {
            Number = number;
        }
    }
}