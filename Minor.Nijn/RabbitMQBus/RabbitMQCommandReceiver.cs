namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandReceiver : ICommandReceiver
    {
        public string QueueName { get; }
        
        public void DeclareCommandQueue()
        {
            throw new System.NotImplementedException();
        }

        public void StartReceivingCommands(CommandReceivedCallback callback)
        {
            throw new System.NotImplementedException();
        }
        
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}