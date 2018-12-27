using RabbitMQ.Client;
using System;
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
            CheckDisposed();
            return new RabbitMQMessageSender(this);
        }

        public IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions)
        {
            CheckDisposed();
            return new RabbitMQMessageReceiver(this, queueName, topicExpressions);
        }
        
        public ICommandSender CreateCommandSender()
        {
            CheckDisposed();
            return new RabbitMQCommandSender(this);
        }

        public ICommandReceiver CreateCommandReceiver(string queueName)
        {
            CheckDisposed();
            return new RabbitMQCommandReceiver(this, queueName);
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
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
