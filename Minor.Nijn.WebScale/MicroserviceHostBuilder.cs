using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly List<IEventListener> _eventListeners;
        private readonly List<ICommandListener> _commandListeners;
        private readonly IServiceCollection _serviceCollection;

        public MicroserviceHostBuilder()
        {
            _callingAssembly = Assembly.GetCallingAssembly();
            _eventListeners = new List<IEventListener>();
            _commandListeners = new List<ICommandListener>();
            _serviceCollection = new ServiceCollection();
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
            foreach (var type in _callingAssembly.GetTypes())
            {
                #if DEBUG
                if (type.Name.StartsWith("Invalid")) continue;
                #endif

                ParseType(type);
            }

            return this;
        }

        /// <summary>
        /// Manually adds EventListeners to the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder AddListener<T>()
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

        private void ParseType(Type type)
        {
            var eventAttribute = type.GetCustomAttribute<EventListenerAttribute>();
            if (eventAttribute != null)
            {
                ParseTopics(type, eventAttribute.QueueName);
            }

            var commandAttribute = type.GetCustomAttribute<CommandListenerAttribute>();
            if (commandAttribute != null)
            {
                ParseCommand(type);
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
                    CreateEventListener(type, method, queueName, attribute.TopicExpressions);
                }
            }
        }

        private void ParseCommand(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<CommandAttribute>();
                if (attribute != null)
                {
                    CreateCommandListener(type, method, attribute.QueueName);
                }
            }
        }

        private void CreateEventListener(Type type, MethodInfo method, string queueName, IEnumerable<string> topicExpressions)
        {
            CheckParameterType(type, method, typeof(DomainEvent));
            _eventListeners.Add(new EventListener(type, method, queueName, topicExpressions));
        }

        private void CreateCommandListener(Type type, MethodInfo method, string queueName)
        {
            CheckParameterType(type, method, typeof(DomainCommand));

            if (method.ReturnType == typeof(void))
            {
                throw new ArgumentException($"Invalid return type by '{method.Name}', return types by command is required");
            }

            _commandListeners.Add(new CommandListener(type, method, queueName));
        }

        private static void CheckParameterType(MemberInfo type, MethodBase method, Type derivedTypeOf)
        {
            var parameters = method.GetParameters();
            if (parameters.Length > 1)
            {
                throw new ArgumentException($"Method '{method.Name}' in type '{type.Name}' has to many parameters");
            }

            if (!parameters.ElementAtOrDefault(0)?.ParameterType.IsSubclassOf(derivedTypeOf) ?? true)
            {
                throw new ArgumentException($"Invalid parameter type in '{method.Name}', parameter has to be derived type of {derivedTypeOf.Name}");
            }
        }

        /// <summary>
        /// Configures Dependency Injection for the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder RegisterDependencies(Action<IServiceCollection> servicesConfiguration)
        {
            servicesConfiguration(_serviceCollection);
            return this;
        }

        /// <summary>
        /// Creates the MicroserviceHost, based on the configurations
        /// </summary>
        /// <returns></returns>
        public IMicroserviceHost CreateHost()
        {
            return new MicroserviceHost(Context, _eventListeners, _commandListeners, _serviceCollection);
        }
    }
}
