using System;
using System.Runtime.Serialization;

namespace Minor.Nijn.TestBus
{
    [Serializable]
    internal class InvalidTopicException : Exception
    {
        public InvalidTopicException()
        {
        }

        public InvalidTopicException(string message) : base(message)
        {
        }

        public InvalidTopicException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidTopicException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}