using System;
using System.Threading;
using System.Threading.Tasks;

namespace Minor.Nijn.TestBus.CommandBus
{
    public sealed class TestCommandSender : ITestCommandSender
    {
        public string ReplyQueueName { get; private set; }

        private readonly ITestBusContext _context;
        private bool _disposed;

        internal TestCommandSender(ITestBusContext context)
        {
            _context = context;
        }
        
        public Task<ResponseCommandMessage> SendCommandAsync(RequestCommandMessage request)
        {
            CheckDisposed();
            ReplyQueueName = Guid.NewGuid().ToString();
            var replyQueue = _context.CommandBus.DeclareCommandQueue(ReplyQueueName);

            var task = StartListeningForCommandReply(replyQueue, request.CorrelationId);
            var command = new TestBusCommand(ReplyQueueName, request);
            _context.CommandBus.DispatchMessage(command);

            return task;
        }

        private static Task<ResponseCommandMessage> StartListeningForCommandReply(CommandBusQueue replyQueue, string correlationId)
        {
           return Task.Run(() =>
            {
                var flag = new ManualResetEvent(false);

                ResponseCommandMessage result = null;
                replyQueue.Subscribe((sender, args) =>
                {
                    if (args.Message.CorrelationId != correlationId) return;

                    result = args.Message.Command as ResponseCommandMessage;
                    flag.Set();
                });

                bool isSet = flag.WaitOne(Constants.CommandResponseTimeoutMs);
                if (!isSet)
                {
                    throw new TimeoutException($"No response received after {Constants.CommandResponseTimeoutMs / 1000} seconds");
                }

                return result;
            });
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