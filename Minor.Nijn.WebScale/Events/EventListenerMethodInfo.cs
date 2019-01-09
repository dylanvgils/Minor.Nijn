using System;
using System.Collections.Generic;
using System.Reflection;

namespace Minor.Nijn.WebScale.Events
{
    public class EventListenerMethodInfo
    {
        /// <summary>
        /// Handle to EventListener method
        /// </summary>
        public MethodInfo Method { get; internal set; }

        /// <summary>
        /// Boolean indicating if method is async
        /// </summary>
        public bool IsAsync { get; internal set; }

        /// <summary>
        /// Command type the command method takes as argument 
        /// </summary>
        public Type EventType { get; internal set; }

        /// <summary>
        /// Topic expressions the EventListener method is listening to
        /// </summary>
        public IEnumerable<string> TopicExpressions { get; internal set; }
    }
}