using System;
using System.Collections.Generic;

namespace Minor.Nijn.WebScale.Events
{
    public class EventListenerInfo
    {
        /// <summary>
        /// Name of the queue the command listener is subscribed to
        /// </summary>
        public string QueueName { get; internal set; }

        /// <summary>
        /// Handle to type containing EventListener methods
        /// </summary>
        public Type Type { get; internal set; }

        /// <summary>
        /// Boolean indicating if the EventListener class should be created as singleton
        /// </summary>
        public bool IsSingleton { get; internal set; }

        /// <summary>
        /// List containing meta data about the EventListener methods
        /// </summary>
        public List<EventListenerMethodInfo> Methods { get; internal set; }
    }
}