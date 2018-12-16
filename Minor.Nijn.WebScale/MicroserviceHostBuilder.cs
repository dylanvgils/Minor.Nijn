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
        private ILogger _logger;

        private IBusContext<IConnection> Context { get; set; }

        private readonly Assembly _callingAssembly;
        private readonly List<IEventListener> _eventListeners;
        private readonly List<ICommandListener> _commandListeners;
        private readonly IServiceCollection _serviceCollection;
        private readonly IDictionary<string, Type> _exceptionTypes;

        public MicroserviceHostBuilder()
        {
            _callingAssembly = Assembly.GetCallingAssembly();
            _eventListeners = new List<IEventListener>();
            _commandListeners = new List<ICommandListener>();
            _serviceCollection = new ServiceCollection();
            _exceptionTypes = new Dictionary<string, Type>();

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
            foreach (var type in _callingAssembly.GetTypes())
            {
                #if DEBUG
                if (type.Name.StartsWith("Invalid")) continue;
                #endif

                ParseType(type);
            }

            _logger.LogInformation("Found {0} EventListeners and {1} CommandListeners", _eventListeners.Count, _commandListeners.Count);
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
        /// Scans te calling assembly for exception types and adds them to the exception type dictionary
        /// </summary>
        /// <returns></returns>
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
        /// <returns></returns>
        public MicroserviceHostBuilder ScanForExceptions(IEnumerable<string> exclusions)
        {
            ScanForExceptionTypes(exclusions.ToList());
            return this;
        }

        /// <summary>
        /// Manually adds exception type to the exception type dictionary
        /// </summary>
        public MicroserviceHostBuilder AddException<T>()
        {
            var type = typeof(T);
            AddExceptionTypeToDictionary(type.Name, type);
            return this;
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
                _logger.LogError("Invalid return type of method: {0}, returning a value from a CommandListener method is required", method.Name);
                throw new ArgumentException($"Invalid return of method: '{method.Name}', returning a value from a CommandListener method is required");
            }

            _commandListeners.Add(new CommandListener(type, method, queueName));
        }

        private void CheckParameterType(MemberInfo type, MethodBase method, Type derivedTypeOf)
        {
            var parameters = method.GetParameters();
            if (parameters.Length > 1)
            {
                _logger.LogError("Method: {0} int type {1} has to many parameters, found {3} expected 1", method.Name, type.Name, parameters.Length);
                throw new ArgumentException($"Method: '{method.Name}' in type: '{type.Name}' has to many parameters");
            }

            if (!parameters.ElementAtOrDefault(0)?.ParameterType.IsSubclassOf(derivedTypeOf) ?? true)
            {
                _logger.LogError("Invalid parameter type in method: {0}, parameter has to be derived type of {1}", method.Name, derivedTypeOf.Name);
                throw new ArgumentException($"Invalid parameter type in method: '{method.Name}', parameter has to be derived type of {derivedTypeOf.Name}");
            }
        }

        private void ScanForExceptionTypes(IReadOnlyCollection<string> exclusions)
        {
            var exceptions = new List<KeyValuePair<string, Type>>(QueryAssemblyForExceptionTypes(_callingAssembly, exclusions));

            foreach (var assemblyName in _callingAssembly.GetReferencedAssemblies())
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
            if (_exceptionTypes.ContainsKey(key))
            {
                _logger.LogError("Unable to add exception to exception type dictionary, exception with name: {0} already exists", value.Name);
                throw new ArgumentException($"Unable to add exception to exception type dictionary, exception with name: {value.Name} already exists");
            }

            _logger.LogDebug("{0} added to exception dictionary", value.FullName);
            _exceptionTypes.Add(key, value);
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
            _logger.LogInformation("Creating MicroserviceHost, {0} dependencies registered", _serviceCollection.Count);
            CommandPublisher.ExceptionTypes = _exceptionTypes;
            return new MicroserviceHost(Context, _eventListeners, _commandListeners, _serviceCollection);
        }
    }
}
