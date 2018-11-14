namespace Minor.Nijn.TestBus.CommandBus
{
    internal sealed class CommandBus : BaseBus<CommandBusQueue, CommandMessage>, ICommandBus
    {        
        public CommandBusQueue DeclareCommandQueue(string queueName)
        {
            return DeclareQueue(queueName, new CommandBusQueue(queueName));
        }
    }
}