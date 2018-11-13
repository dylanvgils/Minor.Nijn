using System.Collections.Generic;

namespace Minor.Nijn.TestBus.CommandBus
{
    internal class CommandBusQueue : TestBusQueue<CommandMessage>
    {
        public CommandBusQueue(string queueName) : base(queueName) { }
    }
}