using System;
using System.Collections.Generic;

namespace Minor.Nijn.WebScale.Events
{
    /// <summary>
    /// Listens to incoming events and invokes the corresponding event listener method
    /// </summary>
    public interface IEventListener : IDisposable
    {
        /// <summary>
        /// Name of the queue the event listener is subscribed to
        /// </summary>
        string QueueName { get; }

        /// <summary>
        /// Topics the event listener is interested in
        /// </summary>
        IEnumerable<string> TopicExpressions { get; }

        /// <summary>
        /// Registers the event listener to the Minor.Nijn event listener
        /// </summary>
        /// <param name="host">Microservice host containing the preferred Minor.Nijn context</param>
        void StartListening(IMicroserviceHost host);
    }
}