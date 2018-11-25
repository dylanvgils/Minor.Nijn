namespace Minor.Nijn.TestBus.CommandBus
{
    public interface ITestCommandSender : ICommandSender
    {
        string ReplyQueueName { get; }
    }
}