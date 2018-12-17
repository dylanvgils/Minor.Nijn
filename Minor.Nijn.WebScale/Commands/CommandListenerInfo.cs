using System;
using System.Reflection;

namespace Minor.Nijn.WebScale.Commands
{
    public class CommandListenerInfo
    {
        /// <summary>
        /// Name of the queue the command listener is subscribed to
        /// </summary>
        public string QueueName { get; internal set; }

        /// <summary>
        /// Handle to type containing CommandListener method
        /// </summary>
        public Type Type { get; internal set; }

        /// <summary>
        /// Handle to CommandListener method
        /// </summary>
        public MethodInfo Method { get; internal set; }
        
        /// <summary>
        /// Boolean indicating if method is async
        /// </summary>
        public bool IsAsyncMethod { get; internal set; }
        
        /// <summary>
        /// Command type the command method takes as argument 
        /// </summary>
        public Type CommandType { get; internal set; }
    }
}
