using System;

namespace Minor.Nijn.TestBus
{
    public sealed class TestBusMessageSender : IMessageSender
    {
        private readonly IBusContextExtension _context;

        private TestBusMessageSender() { }

        internal TestBusMessageSender(IBusContextExtension context)
        {
            _context = context;
        }

        public void SendMessage(EventMessage message)
        {
            _context.TestBuzz.DispatchMessage(message);
        }

        public void Dispose() { }
    }
}
