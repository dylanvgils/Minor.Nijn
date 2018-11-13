namespace Minor.Nijn.TestBus
{
    public class TestBusCommandReceiver : ICommandReceiver
    {
        public string QueueName { get; }
        private IBusContextExtension _context;
        
        private TestBusCommandReceiver() { }

        internal TestBusCommandReceiver(IBusContextExtension context)
        {
            _context = context;
        }
        
        public void DeclareCommandQueue()
        {
            throw new System.NotImplementedException();
        }

        public void StartReceivingCommands(CommandReceivedCallback callback)
        {
            throw new System.NotImplementedException();
        }
        
        public void Dispose() { }
    }
}