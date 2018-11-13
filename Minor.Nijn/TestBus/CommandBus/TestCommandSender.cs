using System.Threading.Tasks;

namespace Minor.Nijn.TestBus.CommandBus
{
    public sealed class TestCommandSender : ICommandSender
    {
        private IBusContextExtension _context;
        
        private TestCommandSender() { }

        internal TestCommandSender(IBusContextExtension context)
        {
            _context = context;
        }
        
        public Task<CommandMessage> SendCommandAsync(CommandMessage request)
        {
            throw new System.NotImplementedException();
        }
        
        public void Dispose() { }
    }
}