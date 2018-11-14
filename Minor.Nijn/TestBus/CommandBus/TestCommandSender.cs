using System;
using System.Threading;
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
            string replyQueueName = request.ReplyTo ?? Guid.NewGuid().ToString();
            var replyQueue = _context.CommandBus.DeclareCommandQueue(replyQueueName);
          
            var task = Task.Run(() =>
            {   
                var flag = new ManualResetEvent(false);
                
                CommandMessage result = null;
                replyQueue.MessageAdded += (sender, args) =>
                {
                    result = args.Message;
                    flag.Set();
                };

                flag.WaitOne(5000);
                return result;
            });

            request.ReplyTo = replyQueueName;
            _context.CommandBus.DispatchMessage(request);
            return task;
        }
        
        public void Dispose() { }
    }
}