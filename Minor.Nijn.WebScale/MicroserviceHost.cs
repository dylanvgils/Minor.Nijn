using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.WebScale
{
    /// <summary>
    /// Listens to incoming events and dispatches them to the appropriate handler
    /// </summary>
    public class MicroserviceHost : IDisposable
    {
        public IBusContext<IConnection> Context { get; set; }
        public MicroserviceHost(IBusContext<IConnection> context)
        {
            Context = context;
        }

        public void Dispose()
        {
            Context?.Connection?.Dispose();
        }
    }
}
