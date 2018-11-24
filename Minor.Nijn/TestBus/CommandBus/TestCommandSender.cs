using System.Threading.Tasks;

namespace Minor.Nijn.TestBus.CommandBus
{
    public sealed class TestCommandSender : IMockCommandSender
    {
        public CommandMessage ReplyMessage { get; set; }
        private ITestBusContext _context;
        
        internal TestCommandSender(ITestBusContext context)
        {
            _context = context;
        }
        
        public Task<CommandMessage> SendCommandAsync(CommandMessage request)
        {
            ReplyMessage.RoutingKey = request.RoutingKey;
            return Task.Run(() => ReplyMessage);
        }
        
        public void Dispose() { }
    }
}