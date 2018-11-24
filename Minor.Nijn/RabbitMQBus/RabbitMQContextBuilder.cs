using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQContextBuilder
    {
        private readonly IConnectionFactory _factory;

        private ILoggerFactory _loggerFactory;
        private ILogger _logger;
        
        public string ExchangeName { get; private set; }
        public string Hostname{ get; private set; }
        public int Port { get; private set; }

        public string Username { get; private set; }
        public string Password { get; private set; }

        public string Type { get; private set; }

        public RabbitMQContextBuilder()
        {
            _loggerFactory = NijnLogger.DefaultFactory;

            _loggerFactory
                .AddSerilog(new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger());

            NijnLogger.LoggerFactory = _loggerFactory;
            _logger = _loggerFactory.CreateLogger<RabbitMQContextBuilder>();
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
            _loggerFactory = loggerFactory;
            
            NijnLogger.LoggerFactory = _loggerFactory;
            _logger = _loggerFactory.CreateLogger<RabbitMQContextBuilder>();
            
            return this;
        }

        public RabbitMQContextBuilder ReadFromEnvironmentVariables()
        {
            // TODO
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
            
            var factory = _factory ?? new ConnectionFactory{ HostName = Hostname, Port = Port };
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

            return new RabbitMQBusContext(connection, ExchangeName);
        }
    }
}
