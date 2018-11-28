using System;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQBusContext : IRabbitMQBusContext
    {
        private bool _disposed;

        public IConnection Connection { get; }
        public string ExchangeName { get; }

        internal RabbitMQBusContext(IConnection connection, string exchangeName)
        {
            Connection = connection;
            ExchangeName = exchangeName;
        }

        public IMessageSender CreateMessageSender()
        {
            return new RabbitMQMessageSender(this);
        }

        public IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions)
        {
            return new RabbitMQMessageReceiver(this, queueName, topicExpressions);
        }
        
        public ICommandSender CreateCommandSender()
        {
            return new RabbitMQCommandSender(this);
        }

        public ICommandReceiver CreateCommandReceiver(string queueName)
        {
            return new RabbitMQCommandReceiver(this, queueName);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RabbitMQBusContext()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Connection?.Dispose();
            }

            _disposed = true;
        }
    }
}
