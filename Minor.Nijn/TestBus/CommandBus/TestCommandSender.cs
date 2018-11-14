using System;
using System.Threading;
using System.Threading.Tasks;

namespace Minor.Nijn.TestBus.CommandBus
{
    public sealed class TestCommandSender : IMockCommandSender
    {
        public CommandMessage ReplyMessage { get; set; }
        private IBusContextExtension _context;
        
        private TestCommandSender() { }

        internal TestCommandSender(IBusContextExtension context)
        {
            _context = context;
        }
        
        public Task<CommandMessage> SendCommandAsync(CommandMessage request)
        {
            ReplyMessage.ReplyTo = request.ReplyTo;
            return Task.Run(() => ReplyMessage);
        }
        
        public void Dispose() { }
    }
}