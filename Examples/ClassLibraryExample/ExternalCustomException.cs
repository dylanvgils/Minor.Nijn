using System;
using System.Runtime.Serialization;

namespace ClassLibraryExample
{
    [Serializable]
    public class ExternalCustomException : Exception
    {
        public ExternalCustomException(string message) : base(message)
        {
        }

        protected ExternalCustomException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
