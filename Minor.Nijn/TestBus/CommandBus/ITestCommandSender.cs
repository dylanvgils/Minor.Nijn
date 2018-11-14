namespace Minor.Nijn.TestBus.CommandBus
{
    public interface ITestCommandSender : ICommandSender
    {
        CommandMessage ReplyMessage { get; set; }
    }
}