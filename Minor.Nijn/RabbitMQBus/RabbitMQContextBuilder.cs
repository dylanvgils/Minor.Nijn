using System;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Minor.Nijn.Helpers;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQContextBuilder
    {
        private readonly IConnectionFactory _factory;
        private ILogger _logger;
        
        public string ExchangeName { get; private set; }
        public string Hostname{ get; private set; }
        public int Port { get; private set; }

        public string Username { get; private set; }
        public string Password { get; private set; }

        public string Type { get; private set; } = Constants.DefaultRabbitMqExchangeType;

        public RabbitMQContextBuilder()
        {
            NijnLogger.DefaultFactory
                .AddSerilog(new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger());

            _logger = NijnLogger.CreateLogger<RabbitMQContextBuilder>();
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

        /// <summary>
        /// Creates a context with 
        ///  - an opened connection (based on HostName, Port, UserName and Password)
        ///  - a declared Topic-Exchange (based on ExchangeName)
        /// </summary>
        /// <returns></returns>
        public IRabbitMQBusContext CreateContext()
        {
            _logger.LogInformation("Creating RabbitMQBusContext for exchange: {0} on host {1}:{2}", ExchangeName, Hostname, Port);
            _logger.LogDebug("Context configuration: type={1}, username={2}, password={3}", Type, Username, Password);

            var connection = CreateConnection();
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

            return new RabbitMQBusContext(connection, ExchangeName);
        }

        private IConnection CreateConnection()
        {
            IConnection connection;
            var factory = _factory ?? new ConnectionFactory { HostName = Hostname, Port = Port };

            try
            {
                connection = factory.CreateConnection();
            }
            catch (Exception e)
            {
                _logger.LogError("Unable to connect to the RabbitMQ host on address: {0}:{1}", Hostname, Port);
                throw new BusConfigurationException("Unable to connect to the RabbitMQ host", e);
            }

            return connection;
        }
    }
}
