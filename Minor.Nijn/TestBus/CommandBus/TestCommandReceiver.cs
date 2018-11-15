namespace Minor.Nijn.TestBus.CommandBus
{
    public sealed class TestCommandReceiver : ICommandReceiver
    {
        public string QueueName { get; }
        private readonly ITestBusContext _context;
        private CommandBusQueue _queue;
        
        private TestCommandReceiver() { }

        internal TestCommandReceiver(ITestBusContext context, string queueName)
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
            _queue.Subscribe((sender, args) => callback(args.Message));
        }
        
        public void Dispose() { }
    }
}