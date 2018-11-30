using System;
using System.Runtime.Serialization;

namespace Minor.Nijn
{
    [Serializable]
    public class BusConfigurationException : Exception
    {
        public BusConfigurationException(string message) : base(message)
        {
        }
        public BusConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}