namespace Minor.Nijn.TestBus.EventBus
{
    public sealed class TestMessageSender : IMessageSender
    {
        private readonly ITestBusContext _context;

        internal TestMessageSender(ITestBusContext context)
        {
            _context = context;
        }

        public void SendMessage(EventMessage message)
        {
            _context.EventBus.DispatchMessage(message);
        }

        public void Dispose()
        {
            // No need to dispose anything in the TestBus
        }
    }
}
