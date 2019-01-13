using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Minor.Nijn.Helpers;
using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Events;
using Minor.Nijn.WebScale.Helpers;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Minor.Nijn.WebScale
{
    /// <summary>
    /// Listens to incoming events and dispatches them to the appropriate handler
    /// </summary>
    public class MicroserviceHost : IMicroserviceHost
    {
        private readonly ILogger _logger;

        private long _listeningFromTimestamp;
        private bool _isRegistered;
        private bool _isListening;
        private bool _disposed;

        public IBusContext<IConnection> Context { get; }
        public IReadOnlyList<IEventListener> EventListeners { get; }
        public IReadOnlyList<ICommandListener> CommandListeners { get; }
        public IServiceProvider ServiceProvider { get;  }

        public MicroserviceHost(IBusContext<IConnection> context, IReadOnlyList<IEventListener> eventListeners, IReadOnlyList<ICommandListener> commandListeners, IServiceCollection serviceCollection)
        {
            _listeningFromTimestamp = 0;

            Context = context;
            EventListeners = eventListeners;
            CommandListeners = commandListeners;

            ConfigurePublisherServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            _logger = NijnWebScaleLogger.CreateLogger<MicroserviceHost>();
        }

        public void RegisterListeners()
        {
            CheckDisposed();
            if (_isRegistered)
            {
                _logger.LogError("EventListeners already created");
                throw new InvalidOperationException("EventListeners already registered");
            }

            _logger.LogInformation("Registering {0} EventListeners", EventListeners.Count);

            EventListeners.ForEach(e => e.RegisterListener(this));

            _isRegistered = true;
        }

        public void StartListening()
        {
            CheckDisposed();
            if (_isListening)
            {
                _logger.LogError("Listeners already listening");
                throw new InvalidOperationException("Listeners already listening");
            }

            if (!_isRegistered)
            {
                RegisterListeners();
            }

            _logger.LogInformation("Registering {0} CommandListeners", CommandListeners.Count);

            EventListeners.ForEach(e => e.StartListening(_listeningFromTimestamp));
            CommandListeners.ForEach(c => c.StartListening(this));

            _isListening = true;
        }

        public void StartListening(long fromTimestamp)
        {
            _listeningFromTimestamp = fromTimestamp;
            StartListening();
        }

        public virtual object CreateInstance(Type type)
        {
            return ActivatorUtilities.CreateInstance(ServiceProvider, type);
        }

        public bool IsConnectionIdle() => Context.IsConnectionIdle();

        private void ConfigurePublisherServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddNijn(Context);
            serviceCollection.AddNijnWebScale();
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MicroserviceHost()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                EventListeners.ForEach(e => e.Dispose());
                CommandListeners.ForEach(c => c.Dispose());
                Context.Dispose();
            }

            _disposed = true;
        }
    }
}
