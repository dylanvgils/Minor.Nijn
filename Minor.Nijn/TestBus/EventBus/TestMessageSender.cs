namespace Minor.Nijn.TestBus.EventBus
{
    public sealed class TestMessageSender : IMessageSender
    {
        private readonly IBusContextExtension _context;

        private TestMessageSender() { }

        internal TestMessageSender(IBusContextExtension context)
        {
            _context = context;
        }

        public void SendMessage(EventMessage message)
        {
            _context.EventBus.DispatchMessage(message);
        }

        public void Dispose() { }
    }
}
