using System.Collections.Generic;

namespace Minor.Nijn.TestBus.CommandBus
{
    internal sealed class CommandBus : ICommandBus
    {
        private readonly IDictionary<string, CommandBusQueue> _queues;
        public int QueueLength => _queues.Count;
        
        public CommandBus()
        {
            _queues = new Dictionary<string, CommandBusQueue>();
        }
        
        public void DispatchMessage(CommandMessage message)
        {
            throw new System.NotImplementedException();
        }

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