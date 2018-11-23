using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Minor.Nijn.WebScale.Attributes;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Minor.Nijn.WebScale
{
    /// <summary>
    /// Creates and Configures a MicroserviceHost
    /// For example:
    ///     var builder = new MicroserviceHostBuilder()
    ///             .SetLoggerFactory(...)
    ///             .RegisterDependencies((services) =>
    ///                 {
    ///                     services.AddTransient<IFoo,Foo>();
    ///                 })
    ///             .WithBusOptions(new BusOptions(exchangeName: "MVM.TestExchange"))
    ///             .UseConventions();
    /// </summary>
    public class MicroserviceHostBuilder
    {
        IBusContext<IConnection> Context { get; set; }

        private readonly Assembly _callingAssembly;
        private readonly List<EventListener> _eventListeners;

        public MicroserviceHostBuilder()
        {
            _callingAssembly = Assembly.GetCallingAssembly();
            _eventListeners = new List<EventListener>();
        }

        /// <summary>
        /// Configures the connection to the message broker
        /// </summary>
        public MicroserviceHostBuilder WithContext(IBusContext<IConnection> context)
        {
            Context = context;
            return this;
        }

        /// <summary>
        /// Scans the assemblies for EventListeners and adds them to the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder UseConventions()
        {
            // Find Event listeners
            foreach (Type type in _callingAssembly.GetTypes())
            {
                ParseType(type);
            }

            return this;
        }

        /// <summary>
        /// Manually adds EventListeners to the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder AddEventListener<T>()
        {
            var type = typeof(T);
            ParseType(type);
            return this;
        }

        /// <summary>
        /// Configures logging functionality for the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            NijnWebScaleLogger.LoggerFactory = loggerFactory;
            return this;
        }

        /// <summary>
        /// Configures Dependency Injection for the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder RegisterDependencies(Action<IServiceCollection> servicesConfiguration)
        {
            return this;
        }

        /// <summary>
        /// Creates the MicroserviceHost, based on the configurations
        /// </summary>
        /// <returns></returns>
        public MicroserviceHost CreateHost()
        {
            // Create host
            var host = new MicroserviceHost(Context, _eventListeners);
            host.RegisterEventListeners();
            return host;
        }

        private void ParseType(Type type)
        {
            var attribute = type.GetCustomAttribute<EventListenerAttribute>();
            if (attribute != null)
            {
                ParseTopics(type, attribute.QueueName);
            }
        }

        private void ParseTopics(Type type, string queueName)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<TopicAttribute>();
                if (attribute != null)
                {
                    CreateEventListener(type, method, queueName, attribute.TopicPattern);
                }
            }
        }

        private void CreateEventListener(Type type, MethodInfo method, string queueName, string topicPattern)
        {
            _eventListeners.Add(new EventListener(type, method, queueName, new List<string> { topicPattern }));
        }
    }
}
