using System;
using System.Collections.Generic;

namespace Minor.Nijn
{
    /// <summary>
    /// Nijn framework context
    /// </summary>
    /// <typeparam name="TConnection"></typeparam>
    public interface IBusContext<TConnection> : IDisposable
    {
        /// <summary>
        /// Connection to the message broker
        /// </summary>
        TConnection Connection { get; }
        
        /// <summary>
        /// Name of the event exchange, all event listeners and senders
        /// created with this context will use this exchange
        /// </summary>
        string ExchangeName { get; }

        /// <summary>
        /// DateTime representing the last time a message was received
        /// </summary>
        DateTime LastMessageReceivedTime { get; }

        /// <summary>
        /// Creates a new message sender
        /// </summary>
        /// <returns>Message sender implementation</returns>
        IMessageSender CreateMessageSender();

        /// <summary>
        /// Creates a new message receiver with the provided queue name and topic expressions
        /// </summary>
        /// <param name="queueName">Name of the queue the event listener should listen to</param>
        /// <param name="topicExpressions">Topic expressions the listener is interested in</param>
        /// <returns>Message receiver implementation</returns>
        IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions);

        /// <summary>
        /// Creates a new command sender
        /// </summary>
        /// <returns>command sender implementation</returns>
        ICommandSender CreateCommandSender();

        /// <summary>
        /// Creates a new command receiver for the provided queue name
        /// </summary>
        /// <param name="queueName">Name of the queue the command listener should listen to</param>
        /// <returns></returns>
        ICommandReceiver CreateCommandReceiver(string queueName);

        /// <summary>
        /// Returns boolean indicating if the timeout is exceeded
        /// </summary>
        /// <returns></returns>
        bool IsConnectionIdle();
    }
}
