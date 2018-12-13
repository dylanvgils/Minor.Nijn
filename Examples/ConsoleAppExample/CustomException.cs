using System;
using System.Runtime.Serialization;

namespace ConsoleAppExample
{
    [Serializable]
    public class CustomException : Exception
    {
        public CustomException(string message) : base(message)
        {
        }

        protected CustomException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
