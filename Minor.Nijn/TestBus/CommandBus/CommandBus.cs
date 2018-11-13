namespace Minor.Nijn.TestBus.CommandBus
{
    internal sealed class CommandBus : BaseBus<CommandBusQueue, CommandMessage>, ICommandBus
    {        
        public CommandBusQueue DeclareCommandQueue(string queueName)
        {       
            if (_queues.ContainsKey(queueName))
            {
                return _queues[queueName];
            }
            
            var queue = new CommandBusQueue(queueName);
            _queues.Add(queueName, queue);
            return queue;
        }
    }
}