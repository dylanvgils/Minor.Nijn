using System.Collections.Generic;

namespace Minor.Nijn.TestBus
{
    internal class BaseBus<TQueueType, TMessageType> : IBus<TMessageType> where TQueueType : TestBusQueue<TMessageType>
    {
        protected readonly IDictionary<string, TQueueType> _queues;
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
    }
}