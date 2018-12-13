using Minor.Nijn.WebScale.Commands;

namespace ConsoleAppExample.Domain
{
    public class SayHelloCommand : DomainCommand
    {
        public string Name { get; }

        public SayHelloCommand(string name, string routingKey) : base(routingKey)
        {
            Name = name;
        }
    }
}
