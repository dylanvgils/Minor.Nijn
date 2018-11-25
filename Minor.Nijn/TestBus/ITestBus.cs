using System.Collections.Generic;

namespace Minor.Nijn.TestBus
{
    public interface ITestBus<TQueueType, in TMessageType>
    {
        IDictionary<string, TQueueType> Queues { get; }
        int QueueCount { get; }

        void DispatchMessage(TMessageType message);
    }
}