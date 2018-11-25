using System.Collections.Generic;

namespace Minor.Nijn.TestBus
{
    public interface ITestBus<TQueueType, in TMessageType> 
        where TQueueType : TestBusQueue<TMessageType>
    {
        IDictionary<string, TQueueType> Queues { get; }
        int QueueCount { get; }

        TQueueType DeclareQueue(string queueName, TQueueType queue);
        void DispatchMessage(TMessageType message);
    }
}