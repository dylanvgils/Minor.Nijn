namespace Minor.Nijn.TestBus.CommandBus
{
    public sealed class TestCommandReceiver : ICommandReceiver
    {
        public string QueueName { get; }
        private readonly IBusContextExtension _context;
        private CommandBusQueue _queue;
        
        private TestCommandReceiver() { }

        internal TestCommandReceiver(IBusContextExtension context, string queueName)
        {
            QueueName = queueName;
            _context = context;
        }
        
        public void DeclareCommandQueue()
        {
            _queue = _context.CommandBus.DeclareCommandQueue(QueueName);
        }

        public void StartReceivingCommands(CommandReceivedCallback callback)
        {
            _queue.MessageAdded += (sender, args) => callback(args.Message);
        }
        
        public void Dispose() { }
    }
}