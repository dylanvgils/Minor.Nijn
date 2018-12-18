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
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Minor.Nijn.WebScale.Helpers;
using DomainEvent = Minor.Nijn.WebScale.Events.DomainEvent;

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
        private ILogger _logger;

        public Assembly CallingAssembly { get; }
        public IBusContext<IConnection> Context { get; private set; }
        public List<IEventListener> EventListeners { get; }
        public List<ICommandListener> CommandListeners { get; }
        public IServiceCollection ServiceCollection { get; }
        public IDictionary<string, Type> ExceptionTypes { get; }

        public MicroserviceHostBuilder()
        {
            CallingAssembly = Assembly.GetCallingAssembly();
            EventListeners = new List<IEventListener>();
            CommandListeners = new List<ICommandListener>();
            ServiceCollection = new ServiceCollection();
            ExceptionTypes = new Dictionary<string, Type>();

            _logger = NijnWebScaleLogger.CreateLogger<MicroserviceHostBuilder>();
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
            foreach (var type in CallingAssembly.GetTypes())
            {
                #if DEBUG
                if (type.Name.StartsWith("Invalid")) continue;
                #endif

                ParseType(type);
            }

            _logger.LogInformation("Found {0} EventListeners and {1} CommandListeners", EventListeners.Count, CommandListeners.Count);
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
            var isAsync = IsAsyncMethod(method);

            if (isAsync && method.ReturnType == typeof(void))
            {
                _logger.LogError("Invalid return type of method: {0}, return type of async method should be Task", method.Name);
                throw new ArgumentException($"Invalid return type of method: {method.Name}, return type of async method should be Task");
            }

            var meta = new EventListenerInfo
            {
                QueueName = queueName,
                TopicExpressions = topicExpressions,
                Type = type,
                Method = method,
                IsAsyncMethod = isAsync,
                EventType = method.GetParameters()[0].ParameterType
            };

            EventListeners.Add(new EventListener(meta));
        }

        private void CreateCommandListener(Type type, MethodInfo method, string queueName)
        {
            CheckParameterType(type, method, typeof(DomainCommand));
            var isAsync = IsAsyncMethod(method);

            if (method.ReturnType == typeof(void))
            {
                _logger.LogError("Invalid return type of method: {0}, returning a value from a CommandListener method is required", method.Name);
                throw new ArgumentException($"Invalid return of method: '{method.Name}', returning a value from a CommandListener method is required");
            }

            if (isAsync && !method.ReturnType.IsGenericType)
            {
                _logger.LogError("Invalid return type of method: {0}, return type of async CommandListener method should be of type Task<T>", method.Name);
                throw new ArgumentException($"Invalid return type of method: {method.Name}, return type of async CommandListener method should be of type Task<T>");
            }

            var meta = new CommandListenerInfo
            {
                QueueName = queueName,
                Type = type,
                Method = method,
                IsAsyncMethod = isAsync,
                CommandType = method.GetParameters()[0].ParameterType
            };

            CommandListeners.Add(new CommandListener(meta));
        }

        private void CheckParameterType(MemberInfo type, MethodBase method, Type derivedTypeOf)
        {
            var parameters = method.GetParameters();
            if (parameters.Length > 1)
            {
                _logger.LogError("Method: {0} int type {1} has to many parameters, found {3} expected 1", method.Name, type.Name, parameters.Length);
                throw new ArgumentException($"Method: '{method.Name}' in type: '{type.Name}' has to many parameters");
            }

            var paramType = parameters[0].ParameterType;
            if (!(paramType == typeof(EventMessage) && derivedTypeOf == typeof(DomainEvent)) && !paramType.IsSubclassOf(derivedTypeOf))
            {
                _logger.LogError("Invalid parameter type in method: {0}, parameter has to be derived type of {1}", method.Name, derivedTypeOf.Name);
                throw new ArgumentException($"Invalid parameter type in method: '{method.Name}', parameter has to be derived type of {derivedTypeOf.Name}");
            }
        }

        private static bool IsAsyncMethod(MemberInfo method)
        {
            return method.GetCustomAttribute<AsyncStateMachineAttribute>() != null;
        }

        /// <summary>
        /// Scans te calling assembly for exception types and adds them to the exception type dictionary
        /// </summary>
        public MicroserviceHostBuilder ScanForExceptions()
        {
            ScanForExceptions(new List<string>());
            return this;
        }

        /// <summary>
        /// Scans the calling assembly for exceptions and exclude the given exclusions, adds the found
        /// exception type to the exception type dictionary
        /// </summary>
        /// <param name="exclusions">Assembly namespace prefixes to exclude</param>
        public MicroserviceHostBuilder ScanForExceptions(IEnumerable<string> exclusions)
        {
            ScanForExceptionTypes(exclusions.ToList());
            return this;
        }

        /// <summary>
        /// Manually adds exception type to the exception type dictionary
        /// </summary>
        public MicroserviceHostBuilder AddException<T>() where T : Exception
        {
            var type = typeof(T);
            AddExceptionTypeToDictionary(type.Name, type);
            return this;
        }

        private void ScanForExceptionTypes(IReadOnlyCollection<string> exclusions)
        {
            var exceptions = new List<KeyValuePair<string, Type>>(QueryAssemblyForExceptionTypes(CallingAssembly, exclusions));

            foreach (var assemblyName in CallingAssembly.GetReferencedAssemblies())
            {
                var assembly = Assembly.Load(assemblyName);
                exceptions.AddRange(QueryAssemblyForExceptionTypes(assembly, exclusions));
            }

            _logger.LogInformation("Found {0} exceptions during exception scan", exceptions.Count);
            exceptions.ForEach(kv => AddExceptionTypeToDictionary(kv.Key, kv.Value));
        }

        private static IEnumerable<KeyValuePair<string, Type>> QueryAssemblyForExceptionTypes(Assembly assembly, IReadOnlyCollection<string> exclusions)
        {
            var query = assembly.GetTypes()
                .Where(type => typeof(Exception).IsAssignableFrom(type)
                               && !Constants.ExceptionScanExclusions.Any(e => type.FullName.StartsWith(e))
                               && !exclusions.Any(e => type.FullName.StartsWith(e)))
                .Select(type => new KeyValuePair<string, Type>(type.Name, type));

            return query.ToList();
        }

        private void AddExceptionTypeToDictionary(string key, Type value)
        {
            if (ExceptionTypes.ContainsKey(key))
            {
                _logger.LogError("Unable to add exception to exception type dictionary, exception with name: {0} already exists", value.Name);
                throw new ArgumentException($"Unable to add exception to exception type dictionary, exception with name: {value.Name} already exists");
            }

            _logger.LogDebug("{0} added to exception dictionary", value.FullName);
            ExceptionTypes.Add(key, value);
        }

        /// <summary>
        /// Configures logging functionality for the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            NijnWebScaleLogger.LoggerFactory = loggerFactory;
            _logger = NijnWebScaleLogger.CreateLogger<MicroserviceHostBuilder>();
            return this;
        }

        /// <summary>
        /// Configures Dependency Injection for the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder RegisterDependencies(Action<IServiceCollection> servicesConfiguration)
        {
            servicesConfiguration(ServiceCollection);
            return this;
        }

        /// <summary>
        /// Creates the MicroserviceHost, based on the configurations
        /// </summary>
        /// <returns></returns>
        public IMicroserviceHost CreateHost()
        {
            _logger.LogInformation("Creating MicroserviceHost, {0} dependencies registered", ServiceCollection.Count);
            CommandPublisher.ExceptionTypes = ExceptionTypes;
            return new MicroserviceHost(Context, EventListeners, CommandListeners, ServiceCollection);
        }
    }
}
