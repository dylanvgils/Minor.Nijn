using System;
using System.Collections.Generic;
using Minor.Nijn.TestBus.CommandBus;
using Minor.Nijn.TestBus.EventBus;
using RabbitMQ.Client;

namespace Minor.Nijn.TestBus
{
    public sealed class TestBusContext : ITestBusContext
    {
        private bool _disposed;

        public IConnection Connection { get; }
        public string ExchangeName => throw new NotImplementedException();

        public DateTime LastMessageReceivedTime { get; private set; } = DateTime.Now;
        public int ConnectionTimeoutMs { get; }

        public IEventBus EventBus { get; }
        public ICommandBus CommandBus { get; }

        internal TestBusContext(IConnection connection, IEventBus testBus, ICommandBus commandBus, int idleAfterMs)
        {
            Connection = connection;
            EventBus = testBus;
            CommandBus = commandBus;
            ConnectionTimeoutMs = idleAfterMs;
        }

        public IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions)
        {
            CheckDisposed();
            return new TestMessageReceiver(this, queueName, topicExpressions);
        }

        public IMessageSender CreateMessageSender()
        {
            CheckDisposed();
            return new TestMessageSender(this);
        }

        public ICommandReceiver CreateCommandReceiver(string queueName)
        {
            CheckDisposed();
            return new TestCommandReceiver(this, queueName);
        }

        public ICommandSender CreateCommandSender()
        {
            CheckDisposed();
            return new TestCommandSender(this);
        }
        public void UpdateLastMessageReceived()
        {
            LastMessageReceivedTime = DateTime.Now;
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
            // No need to dispose anything in the TestBus
            _disposed = true;
        }
    }
}
