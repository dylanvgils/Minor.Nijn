using System;

namespace Minor.Nijn
{
    /// <summary>
    /// Sender for event messages
    /// </summary>
    public interface IMessageSender : IDisposable
    {
        /// <summary>
        /// Sends event message
        /// </summary>
        /// <param name="message">event message to send</param>
        void SendMessage(EventMessage message);
    }
}
