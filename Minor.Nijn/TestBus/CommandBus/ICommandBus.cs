namespace Minor.Nijn.TestBus.CommandBus
{
    internal interface ICommandBus : IBus<CommandMessage>
    {
        CommandBusQueue DeclareCommandQueue(string queueName);
    }
}