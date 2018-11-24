using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Minor.Nijn.WebScale.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Minor.Nijn.WebScale
{
    /// <summary>
    /// Listens to incoming events and dispatches them to the appropriate handler
    /// </summary>
    public class MicroserviceHost : IMicroserviceHost
    {
        private readonly ILogger _logger;

        public IBusContext<IConnection> Context { get; }
        public List<IEventListener> EventListeners { get; }
        public bool EventListenersRegistered { get; private set; }
        public IServiceProvider ServiceProvider { get;  }

        public MicroserviceHost(IBusContext<IConnection> context, List<IEventListener> eventListeners, IServiceCollection serviceCollection)
        {
            Context = context;
            EventListeners = eventListeners;

            ConfigurePublisherServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            _logger = NijnWebScaleLogger.CreateLogger<MicroserviceHost>();
        }

        public void RegisterListeners()
        {
            if (EventListenersRegistered)
            {
                _logger.LogError("Event listeners already created");
                throw new InvalidOperationException("Event listeners already registered");
            }

            _logger.LogInformation("Registering {0} event listeners", EventListeners.Count());
            EventListeners.ForEach(e => e.StartListening(this));
            EventListenersRegistered = true;
        }

        public virtual object CreateInstance(Type type)
        {
            return ActivatorUtilities.CreateInstance(ServiceProvider, type);
        }

        private void ConfigurePublisherServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(Context);
            serviceCollection.AddTransient<IEventPublisher, EventPublisher>();
        }

        public void Dispose()
        {
            EventListeners.ForEach(e => e.Dispose());
            Context.Dispose();
        }
    }
}
