namespace Minor.Nijn.TestBus.CommandBus
{
    public interface IMockCommandSender : ICommandSender
    {
        CommandMessage ReplyMessage { get; set; }
    }
}