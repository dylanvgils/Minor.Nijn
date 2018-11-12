using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQContextBuilder
    {
        private string ExchangeName { get; set; }
        private string Hostname{ get;  set; }
        private int Port { get; set; }

        private string Username { get; set; }
        private string Password { get; set; }

        private string Type { get; set; }

        public RabbitMQContextBuilder WithExchange(string exchangeName)
        {
            ExchangeName = exchangeName;
            return this;    // for method chaining
        }

        public RabbitMQContextBuilder WithAddress(string hostName, int port)
        {
            Hostname = hostName;
            Port = port;
            return this;    // for method chaining
        }

        public RabbitMQContextBuilder WithCredentials(string userName, string password)
        {
            Username = userName;
            Password = password;
            return this;    // for method chaining
        }

        public RabbitMQContextBuilder ReadFromEnvironmentVariables()
        {
            // TODO
            return this;    // for method chaining
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
        public RabbitMQBusContext CreateContext()
        {
            var factory = new ConnectionFactory() { HostName = Hostname, Port = Port };
            var connection = factory.CreateConnection();

            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: ExchangeName, type: Type);
            }

            return new RabbitMQBusContext(connection, ExchangeName);
        }
    }
}
