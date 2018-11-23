namespace Minor.Nijn.TestBus.CommandBus
{
    public interface ICommandBus : ITestBus<CommandMessage>
    {
        CommandBusQueue DeclareCommandQueue(string queueName);
    }
}