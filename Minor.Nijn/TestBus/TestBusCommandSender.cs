using System.Threading.Tasks;

namespace Minor.Nijn.TestBus
{
    public class TestBusCommandSender : ICommandSender
    {
        private IBusContextExtension _context;
        
        private TestBusCommandSender() { }

        internal TestBusCommandSender(IBusContextExtension context)
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