using System;

namespace Minor.Nijn
{
    /// <summary>
    /// Receiver for commands
    /// </summary>
    public interface ICommandReceiver : IDisposable
    {
        /// <summary>
        /// Name of the command queue the command receiver is subscribed to
        /// </summary>
        string QueueName { get; }

        /// <summary>
        /// Declares the queue on the message broker
        /// </summary>
        void DeclareCommandQueue();

        /// <summary>
        /// Registers the provided callback to the message broker and
        /// starts listening for commands.
        /// </summary>
        /// <param name="callback"></param>
        void StartReceivingCommands(CommandReceivedCallback callback);
    }

    public delegate ResponseCommandMessage CommandReceivedCallback(RequestCommandMessage request);
}
