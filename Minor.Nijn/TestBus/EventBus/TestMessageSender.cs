using System;

namespace Minor.Nijn.TestBus.EventBus
{
    public sealed class TestMessageSender : IMessageSender
    {
        private readonly ITestBusContext _context;
        private bool _disposed;

        internal TestMessageSender(ITestBusContext context)
        {
            _context = context;
        }

        public void SendMessage(EventMessage message)
        {
            CheckDisposed();
            _context.EventBus.DispatchMessage(message);
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            // No need to dispose anything in the TestBus
            _disposed = true;
        }
    }
}
