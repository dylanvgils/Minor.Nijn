using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn
{
    public interface ICommandReplySender : IDisposable
    {
        void SendCommandReply(CommandMessage request);
    }
}
