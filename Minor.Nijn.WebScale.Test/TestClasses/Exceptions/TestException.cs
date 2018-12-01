using System;
using System.Runtime.Serialization;

namespace Minor.Nijn.WebScale.TestClasses.Exceptions.Test
{
    [Serializable]
    public class TestException : Exception
    {
        public TestException(string message) : base(message)
        {
        }

        protected TestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
