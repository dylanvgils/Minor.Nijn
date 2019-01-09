using Microsoft.Extensions.Logging;
using Minor.Nijn.Helpers;
using RabbitMQ.Client;
using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQContextBuilder : IRabbitMQContextBuilder
    {
        private readonly IConnectionFactory _factory;
        private ILogger _logger;
        
        public string ExchangeName { get; private set; }
        public string Hostname{ get; private set; }
        public int Port { get; private set; }

        public int ConnectionTimeoutAfterMs { get; private set; } = Constants.RabbitMQConnectionTimeoutAfterMs;
        public bool AutoDisconnectEnabled { get; private set; } 

        public string Username { get; private set; }
        public string Password { get; private set; }

        public string Type { get; private set; } = Constants.DefaultRabbitMqExchangeType;

        internal bool CreateContextAllowed { get; set; }

        public RabbitMQContextBuilder()
        {
            CreateContextAllowed = true;
            _logger = NijnLogger.CreateLogger<RabbitMQContextBuilder>();
        }

        internal RabbitMQContextBuilder(IServiceCollection services) : this()
        {
            CreateContextAllowed = false;
        }

        internal RabbitMQContextBuilder(IConnectionFactory factory) : this()
        {
            _factory = factory;
        }
        
        public RabbitMQContextBuilder WithExchange(string exchangeName)
        {
            ExchangeName = exchangeName;
            return this;
        }

        public RabbitMQContextBuilder WithAddress(string hostName, int port)
        {
            Hostname = hostName;
            Port = port;
            return this;
        }

        public RabbitMQContextBuilder WithConnectionTimeout(int timeoutAfterMs, bool autoDisconnect = false)
        {
            ConnectionTimeoutAfterMs = timeoutAfterMs;
            AutoDisconnectEnabled = autoDisconnect;
            return this;
        }

        public RabbitMQContextBuilder WithCredentials(string userName, string password)
        {
            Username = userName;
            Password = password;
            return this;
        }

        public RabbitMQContextBuilder SetLoggerFactory(ILoggerFactory loggerFactory)
        {            
            NijnLogger.LoggerFactory = loggerFactory;
            _logger = NijnLogger.CreateLogger<RabbitMQContextBuilder>();
            
            return this;
        }

        public RabbitMQContextBuilder ReadFromEnvironmentVariables()
        {
            EnvironmentHelper.GetValue(Constants.EnvExchangeName, v => ExchangeName = v);
            EnvironmentHelper.GetValue(Constants.EnvHostname, v => Hostname = v);
            EnvironmentHelper.GetValue(Constants.EnvUsername, v => Username = v);
            EnvironmentHelper.GetValue(Constants.EnvPassword, v => Password = v);
            EnvironmentHelper.GetValue(Constants.EnvExchangeType, v => Type = v, Constants.DefaultRabbitMqExchangeType);
            EnvironmentHelper.GetValue(Constants.EnvPort, v =>
            {
                try
                {
                    Port = int.Parse(v);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Invalid environment variable: {Constants.EnvPort}, could not parse value to int");
                }
            });

            return this;
        }

        public RabbitMQContextBuilder WithType(string type)
        {
            Type = type;
            return this;
        }

        internal IRabbitMQBusContext CreateContextWithRetry(int times, int retryAfter)
        {
            _logger.LogInformation("Creating RabbitMQBusContext for exchange: {0} on host {1}:{2}", ExchangeName, Hostname, Port);
            _logger.LogInformation("RabbitMQ connection timeout after: {0} ms, auto disconnect enabled: {1}", ConnectionTimeoutAfterMs, AutoDisconnectEnabled);
            _logger.LogDebug("Context configuration: type={1}, username={2}", Type, Username);

            for (var i = 0; i < times; i++)
            {
                try
                {
                    return CreateConnection();
                }
                catch (Exception e)
                {
                    _logger.LogInformation("Unable to connect to the RabbitMQ host error message, '{0}'.Retrying...", e.Message);
                }

                Thread.Sleep(retryAfter);
            }

            throw new BusConfigurationException("Connecting to the RabbitMQ host failed");
        }

        /// <summary>
        /// Creates a context with 
        ///  - an opened connection (based on HostName, Port, UserName and Password)
        ///  - a declared Topic-Exchange (based on ExchangeName)
        /// </summary>
        public IRabbitMQBusContext CreateContext()
        {
            if (!CreateContextAllowed)
            {
                _logger.LogError("CreateContext is not allowed in AddNijn extension method");
                throw new InvalidOperationException("CreateContext is not allowed in AddNijn extension method");
            }

            _logger.LogInformation("Creating RabbitMQBusContext for exchange: {0} on host {1}:{2}", ExchangeName, Hostname, Port);
            _logger.LogDebug("Context configuration: type={1}, username={2}", Type, Username);

            try
            {
                return CreateConnection();
            }
            catch (Exception e)
            {
                _logger.LogError("Unable to connect to the RabbitMQ host on address: {0}:{1}", Hostname, Port);
                throw new BusConfigurationException("Unable to connect to the RabbitMQ host", e);
            }
        }

        private IRabbitMQBusContext CreateConnection()
        {
            var factory = _factory ?? new ConnectionFactory { HostName = Hostname, Port = Port };
            var connection = factory.CreateConnection();

            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(
                    exchange: ExchangeName,
                    type: Type,
                    durable: false,
                    autoDelete: false,
                    arguments: null
                );
            }

            return new RabbitMQBusContext(connection, ExchangeName, ConnectionTimeoutAfterMs, AutoDisconnectEnabled);
        }
    }
}
