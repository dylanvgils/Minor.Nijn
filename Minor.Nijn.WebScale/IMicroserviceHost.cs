using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Minor.Nijn.WebScale
{
    /// <summary>
    /// Nijn.WebScale framework context
    /// </summary>
    public interface IMicroserviceHost : IDisposable
    {
        /// <summary>
        /// Handle to Minor.Nijn IBusContext.
        /// <see cref="IBusContext{TConnection}" /> for more information.
        /// </summary>
        IBusContext<IConnection> Context { get; }

        /// <summary>
        /// Readonly list of all registered EventListeners.
        /// </summary>
        IReadOnlyList<IEventListener> EventListeners { get; }

        /// <summary>
        /// Readonly list of all registered CommandListeners.
        /// </summary>
        IReadOnlyList<ICommandListener> CommandListeners { get; }

        /// <summary>
        /// DI ServiceProvided, containing registered services.
        /// <see cref="IServiceProvider" /> for more information.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Binds all registered EventListeners to the exchange.
        /// </summary>
        void RegisterListeners();

        /// <summary>
        /// Starts receiving messages on all registered Event and CommandListeners.
        /// </summary>
        void StartListening();

        /// <summary>
        /// Starts receiving messages on all registered Event and CommandListeners.
        /// The EventListeners will only accept events from the given from timestamp.
        /// </summary>
        /// <param name="fromTimestamp"></param>
        void StartListening(long fromTimestamp);

        /// <summary>
        /// Creates a new instance of the provided type using the ActivatorUtilities.
        /// <see cref="Microsoft.Extensions.DependencyInjection.ActivatorUtilities"/> for more information.
        /// </summary>
        /// <param name="type">Type to create instance of</param>
        /// <returns>The instantiated type</returns>
        object CreateInstance(Type type);

        /// <summary>
        /// Calls IsConnectionIdle on the IBusContext, which checks if the provided timeout is exceeded.
        /// <see cref="IBusContext{TConnection}" /> for more information.
        /// </summary>
        /// <returns>True if connection is idle and False when the connection is not idle</returns>
        bool IsConnectionIdle();
    }
}
