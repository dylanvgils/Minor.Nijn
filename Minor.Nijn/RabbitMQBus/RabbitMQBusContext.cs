using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQBusContext : IRabbitMQBusContext
    {
        private readonly ILogger _logger;
        private bool _disposed;

        public IConnection Connection { get; }
        public string ExchangeName { get; }

        public bool AutoDisconnectEnabled { get;  }
        public int ConnectionTimeoutMs { get; }
        public DateTime LastMessageReceivedTime { get; private set; } = DateTime.Now;

        internal RabbitMQBusContext(IConnection connection, string exchangeName, int idleAfterMs, bool autoDisconnect)
        {
            Connection = connection;
            ExchangeName = exchangeName;
            ConnectionTimeoutMs = idleAfterMs;
            AutoDisconnectEnabled = autoDisconnect;

            StartAutoDisconnectThread();
            _logger = NijnLogger.CreateLogger<RabbitMQBusContext>();
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

        public void UpdateLastMessageReceived()
        {
            LastMessageReceivedTime = DateTime.Now;
        }

        private void StartAutoDisconnectThread()
        {
            if (!AutoDisconnectEnabled)
            {
                return;
            }

            ThreadPool.QueueUserWorkItem(state =>
            {
                while (!_disposed)
                {
                    if (IsConnectionIdle())
                    {
                        _logger.LogInformation("Closing RabbitMQ connection with exchange {0}", ExchangeName);
                        Dispose(true);
                        return;
                    }

                    Thread.Sleep(ConnectionTimeoutMs);
                }
            });
        }

        public bool IsConnectionIdle()
        {
            return DateTime.Now - LastMessageReceivedTime > TimeSpan.FromMilliseconds(ConnectionTimeoutMs);
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
