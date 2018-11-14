using System.Collections.Generic;

namespace Minor.Nijn.TestBus
{
    internal abstract class BaseBus<TQueueType, TMessageType> : IBus<TMessageType> 
        where TQueueType : TestBusQueue<TMessageType>
    {
        private readonly IDictionary<string, TQueueType> _queues;
        public int QueueLength => _queues.Count;

        protected BaseBus()
        {
            _queues = new Dictionary<string, TQueueType>();
        }
        
        public void DispatchMessage(TMessageType message)
        {
            foreach (var queue in _queues)
            {
                queue.Value.Enqueue(message);
            }
        }

        protected TQueueType DeclareQueue(string queueName, TQueueType queue)
        {
            if (_queues.ContainsKey(queueName))
            {
                return _queues[queueName];
            }

            _queues.Add(queueName, queue);
            return queue;
        }
    }
}