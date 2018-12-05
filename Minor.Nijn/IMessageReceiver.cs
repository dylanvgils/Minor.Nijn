using System;
using System.Collections.Generic;

namespace Minor.Nijn
{
    /// <summary>
    /// Receiver for event messages
    /// </summary>
    public interface IMessageReceiver : IDisposable
    {
        /// <summary>
        /// Name of the queue the event receiver should subscribe to
        /// </summary>
        string QueueName { get; }

        /// <summary>
        /// Topics the event the event receiver is interested in
        /// </summary>
        IEnumerable<string> TopicExpressions { get; }

        /// <summary>
        /// Declares the queue on the message broker
        /// </summary>
        void DeclareQueue();
        
        /// <summary>
        /// Registers the provided callback to the message broker and
        /// starts listening for event messages
        /// </summary>
        /// <param name="callback"></param>
        void StartReceivingMessages(EventMessageReceivedCallback callback);
    }

    public delegate void EventMessageReceivedCallback(EventMessage eventMessage);
}
