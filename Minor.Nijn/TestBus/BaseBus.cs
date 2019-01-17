using System.Collections.Generic;

namespace Minor.Nijn.TestBus
{
    internal abstract class BaseBus<TQueueType, TMessageType> : ITestBus<TQueueType, TMessageType> 
        where TQueueType : TestBusQueue<TMessageType>
    {
        public IDictionary<string, TQueueType> Queues { get; }
        public int QueueCount => Queues.Count;

        protected BaseBus()
        {
            Queues = new Dictionary<string, TQueueType>();
        }
        
        public virtual void DispatchMessage(TMessageType message)
        {
            foreach (var queue in Queues)
            {
                queue.Value.Enqueue(message);
            }
        }

        public virtual TQueueType DeclareQueue(string queueName, TQueueType queue)
        {
            if (Queues.ContainsKey(queueName))
            {
                return Queues[queueName];
            }

            Queues.Add(queueName, queue);
            return queue;
        }
    }
}