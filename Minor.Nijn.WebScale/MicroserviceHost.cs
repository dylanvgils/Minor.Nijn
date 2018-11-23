using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using Minor.Nijn.WebScale.Events;

namespace Minor.Nijn.WebScale
{
    /// <summary>
    /// Listens to incoming events and dispatches them to the appropriate handler
    /// </summary>
    public class MicroserviceHost : IDisposable
    {
        private readonly ILogger _logger;

        public IBusContext<IConnection> Context { get; }
        public List<IEventListener> EventListeners { get; }
        public bool EventListenersRegistered { get; private set; }

        public MicroserviceHost(IBusContext<IConnection> context, List<IEventListener> eventListeners)
        {
            Context = context;
            EventListeners = eventListeners;

            _logger = NijnWebScaleLogger.CreateLogger<MicroserviceHost>();
        }

        public void RegisterEventListeners()
        {
            if (EventListenersRegistered)
            {
                _logger.LogError("Event listeners already created");
                throw new InvalidOperationException("Event listeners already registered");
            }

            _logger.LogInformation("Registering {0} event listeners", EventListeners.Count());
            EventListeners.ForEach(e => e.StartListening(Context));
            EventListenersRegistered = true;
        }

        public void Dispose()
        {
            EventListeners.ForEach(e => e.Dispose());
            Context?.Connection?.Dispose();
        }
    }
}
