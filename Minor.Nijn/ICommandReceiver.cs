using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn
{
    public interface ICommandReceiver : IDisposable
    {
        string QueueName { get; }
        void DeclareCommandQueue();
        void StartReceivingCommands(CommandReceivedCallback callback);
    }

    public delegate CommandMessage CommandReceivedCallback(CommandMessage commandMessage);
}
