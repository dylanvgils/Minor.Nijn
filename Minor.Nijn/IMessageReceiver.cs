using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn
{
    public interface IMessageReceiver : IDisposable
    {
        string QueueName { get; }
        IEnumerable<string> TopicExpressions { get; }

        void DeclareQueue();
        void StartReceivingMessages(EventMessageReceivedCallback callback);
    }

    public delegate void EventMessageReceivedCallback(EventMessage eventMessage);
}
