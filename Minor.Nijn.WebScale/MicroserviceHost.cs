﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using Minor.Nijn.Helpers;
using Minor.Nijn.WebScale.Helpers;

namespace Minor.Nijn.WebScale
{
    /// <summary>
    /// Listens to incoming events and dispatches them to the appropriate handler
    /// </summary>
    public class MicroserviceHost : IMicroserviceHost
    {
        private readonly ILogger _logger;
        private bool _isRegistered;
        private bool _isListening;
        private bool _disposed;

        public IBusContext<IConnection> Context { get; }
        public List<IEventListener> EventListeners { get; }
        public List<ICommandListener> CommandListeners { get; }
        public IServiceProvider ServiceProvider { get;  }

        public MicroserviceHost(IBusContext<IConnection> context, List<IEventListener> eventListeners, List<ICommandListener> commandListeners, IServiceCollection serviceCollection)
        {
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
                _logger.LogError("Listeners already created");
                throw new InvalidOperationException("Listeners already registered");
            }

            _logger.LogInformation("Registering {0} EventListeners and {1} CommandListeners", EventListeners.Count, CommandListeners.Count);

            EventListeners.ForEach(e => e.RegisterListener(this));
            CommandListeners.ForEach(c => c.RegisterListener(this));

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

            EventListeners.ForEach(e => e.StartListening());
            CommandListeners.ForEach(c => c.StartListening());

            _isListening = true;
        }

        public virtual object CreateInstance(Type type)
        {
            return ActivatorUtilities.CreateInstance(ServiceProvider, type);
        }

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
