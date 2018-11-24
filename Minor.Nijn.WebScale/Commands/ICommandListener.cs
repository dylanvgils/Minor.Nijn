using System;

namespace Minor.Nijn.WebScale.Commands
{
    public interface ICommandListener : IDisposable
    {
        string QueueName { get; }

        void StartListening(IMicroserviceHost host);
    }
}