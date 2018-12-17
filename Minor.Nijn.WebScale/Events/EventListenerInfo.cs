using System;
using System.Collections.Generic;
using System.Reflection;

namespace Minor.Nijn.WebScale.Events
{
    public class EventListenerInfo
    {
        /// <summary>
        /// Name of the queue the command listener is subscribed to
        /// </summary>
        public string QueueName { get; internal set; }

        /// <summary>
        /// Topics the event listener is interested in
        /// </summary>
        public IEnumerable<string> TopicExpressions { get; internal set; }

        /// <summary>
        /// Handle to type containing EventListener method
        /// </summary>
        public Type Type { get; internal set; }


        /// <summary>
        /// Handle to EventListener method
        /// </summary>
        public MethodInfo Method { get; internal set; }

        /// <summary>
        /// Boolean indicating if method is async
        /// </summary>
        public bool IsAsyncMethod { get; internal set; }

        /// <summary>
        /// Command type the command method takes as argument 
        /// </summary>
        public Type EventType { get; internal set; }
    }
}