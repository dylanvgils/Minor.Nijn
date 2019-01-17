using System;
using System.Collections.Generic;

namespace Minor.Nijn.TestBus.EventBus
{
    public sealed class TestMessageReceiver : IMessageReceiver
    {
        public string QueueName { get; }
        public IEnumerable<string> TopicExpressions { get; }
        private readonly ITestBusContext _context;

        private EventBusQueue _queue;
        private bool _queueDeclared;
        private bool _disposed;

        internal TestMessageReceiver(ITestBusContext context, string queueName, IEnumerable<string> topicExpressions)
        {
            _context = context;
            QueueName = queueName;
            TopicExpressions = topicExpressions;
        }

        public void DeclareQueue()
        {
            CheckDisposed();
            if (_queueDeclared)
            {
                throw new BusConfigurationException($"Queue with name: {QueueName} is already declared");
            }

            _queue =  _context.EventBus.DeclareQueue(QueueName, TopicExpressions);
           _queueDeclared = true;
        }

        public void StartReceivingMessages(EventMessageReceivedCallback callback)
        {
            CheckDisposed();
            if (!_queueDeclared)
            {
                throw new BusConfigurationException($"Queue with name: {QueueName} is not declared");
            }

            _queue.Subscribe((sender, args) =>
            {
                callback(args.Message);
                _context.UpdateLastMessageReceived();
            });
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
