using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQContextBuilder
    {
        private readonly IConnectionFactory _factory;
        
        public string ExchangeName { get; private set; }
        public string Hostname{ get; private set; }
        public int Port { get; private set; }

        public string Username { get; private set; }
        public string Password { get; private set; }

        public string Type { get; private set; }
        
        public RabbitMQContextBuilder() { }
        
        internal RabbitMQContextBuilder(IConnectionFactory factory)
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
