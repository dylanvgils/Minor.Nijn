namespace Minor.Nijn.TestBus.CommandBus
{
    public class TestBusCommand
    {
        public CommandMessage Command { get; }
        public string CorrelationId => Command.CorrelationId;
        public string RoutingKey => Command.RoutingKey;
        public string ReplyTo { get;  }

        public TestBusCommand(string replyTo, CommandMessage command)
        {
            ReplyTo = replyTo;
            Command = command;
        }
    }
}