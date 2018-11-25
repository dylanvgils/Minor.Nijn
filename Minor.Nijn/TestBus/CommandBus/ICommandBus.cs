namespace Minor.Nijn.TestBus.CommandBus
{
    public interface ICommandBus : ITestBus<CommandBusQueue, CommandMessage>
    {
        CommandBusQueue DeclareCommandQueue(string queueName);
    }
}