using System;
using System.Threading;
using System.Threading.Tasks;

namespace Minor.Nijn.TestBus.CommandBus
{
    public sealed class TestCommandSender : ITestCommandSender
    {
        public string ReplyQueueName { get; private set; }
        private readonly ITestBusContext _context;
        
        internal TestCommandSender(ITestBusContext context)
        {
            _context = context;
        }
        
        public Task<CommandMessage> SendCommandAsync(CommandMessage request)
        {
            ReplyQueueName = Guid.NewGuid().ToString();
            var replyQueue = _context.CommandBus.DeclareCommandQueue(ReplyQueueName);

            var task = StartListeningForCommandReply(replyQueue);
            var command = new TestBusCommand(ReplyQueueName, request);
            _context.CommandBus.DispatchMessage(command);

            return task;
        }

        private static Task<CommandMessage> StartListeningForCommandReply(CommandBusQueue replyQueue)
        {
           return Task.Run(() =>
            {
                var flag = new ManualResetEvent(false);

                CommandMessage result = null;
                replyQueue.Subscribe((sender, args) =>
                {
                    result = args.Message.Command;
                    flag.Set();
                });

                bool isSet = flag.WaitOne(5000);
                if (!isSet)
                {
                    throw new TimeoutException("No response received after 5 seconds");
                }

                return result;
            });
        }

        public void Dispose() { }
    }
}