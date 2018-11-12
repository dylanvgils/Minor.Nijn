using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn
{
    public class CommandMessage
    {
        public string Message { get; }
        public string Type { get; }
        public string CorrelationId { get; }
    }
}
