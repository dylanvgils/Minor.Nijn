using System;

namespace Minor.Nijn.WebScale.Commands
{
    /// <summary>
    /// Listens to incoming commands and invokes the corresponding listener method.
    /// </summary>
    public interface ICommandListener : IDisposable
    {
        /// <summary>
        /// Name of the queue the command listener is subscribed to
        /// </summary>
        string QueueName { get; }

        /// <summary>
        /// Registers the command listener to the Minor.Nijn command listener
        /// </summary>
        /// <param name="host">Microservice host containing the preferred Minor.Nijn context</param>
        void StartListening(IMicroserviceHost host);
    }
}