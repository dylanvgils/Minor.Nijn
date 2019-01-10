using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Minor.Nijn.WebScale
{
    public interface IMicroserviceHost : IDisposable
    {
        IBusContext<IConnection> Context { get; }
        List<IEventListener> EventListeners { get; }
        List<ICommandListener> CommandListeners { get; }
        IServiceProvider ServiceProvider { get; }

        void RegisterListeners();
        void StartListening();
        void StartListening(long fromTimestamp);
        object CreateInstance(Type type);
    }
}
