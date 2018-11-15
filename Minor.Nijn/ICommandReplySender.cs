using System;

namespace Minor.Nijn
{
    public interface ICommandReplySender : IDisposable
    {
        void SendCommandReply(CommandMessage request);
    }
}
