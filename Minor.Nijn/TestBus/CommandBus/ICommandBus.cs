namespace Minor.Nijn.TestBus.CommandBus
{
    public interface ICommandBus : ITestBus<CommandBusQueue, TestBusCommand>
    {
        CommandBusQueue DeclareCommandQueue(string queueName);
    }
}