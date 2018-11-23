using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Minor.Nijn.WebScale
{
    /// <summary>
    /// Listens to incoming events and dispatches them to the appropriate handler
    /// </summary>
    public class MicroserviceHost : IDisposable
    {
        private readonly ILogger _logger;

        public IBusContext<IConnection> Context { get; }
        public IEnumerable<EventListener> EventListeners { get; }
        public bool EventListenersRegistered { get; private set; }

        public MicroserviceHost(IBusContext<IConnection> context, IEnumerable<EventListener> eventListeners)
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

            foreach (var listener in EventListeners)
            {
                var receiver = Context.CreateMessageReceiver(
                    listener.QueueName,
                    listener.TopicPatterns
                );

                receiver.DeclareQueue();
                receiver.StartReceivingMessages(listener.HandleEventMessage);
            }

            EventListenersRegistered = true;
        }

        public void Dispose()
        {
            Context?.Connection?.Dispose();
        }
    }
}
