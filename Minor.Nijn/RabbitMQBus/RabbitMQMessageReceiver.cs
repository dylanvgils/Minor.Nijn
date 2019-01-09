using System;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;
using Minor.Nijn.Helpers;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQMessageReceiver : IMessageReceiver
    {
        private readonly ILogger _logger;
        
        public string QueueName { get; }
        public IEnumerable<string> TopicExpressions { get; }
        public IModel Channel { get; }

        private bool _queueDeclared;
        private bool _disposed;

        private readonly IRabbitMQBusContext _context;
        private readonly EventingBasicConsumerFactory _eventingBasicConsumerFactory;

        internal RabbitMQMessageReceiver(IRabbitMQBusContext context, string queueName,
            IEnumerable<string> topicExpressions, EventingBasicConsumerFactory factory) : this(context, queueName, topicExpressions)
        {
            _eventingBasicConsumerFactory = factory;
        }
        
        internal RabbitMQMessageReceiver(IRabbitMQBusContext context, string queueName, IEnumerable<string> topicExpressions)
        {
            _context = context;
            QueueName = queueName;
            TopicExpressions = topicExpressions;
            Channel = _context.Connection.CreateModel();

            _eventingBasicConsumerFactory = new EventingBasicConsumerFactory();
            
            _logger = NijnLogger.CreateLogger<RabbitMQMessageReceiver>();
        }

        public void DeclareQueue()
        {
            CheckDisposed();
            CheckQueueAlreadyDeclared();
            TopicMatcher.AreValidTopicExpressions(TopicExpressions);

            _logger.LogInformation("Declare queue on exchange with name: {0}", QueueName);
            
            Channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            foreach (var topic in TopicExpressions)
            {
                _logger.LogInformation("Bind queue {0} to exchange {1} with with topic {2}", QueueName, _context.ExchangeName, topic);

                Channel.QueueBind(
                    queue: QueueName,
                    exchange: _context.ExchangeName,
                    routingKey: topic, 
                    arguments: null
                );
            }

            _queueDeclared = true;
        }

        public void StartReceivingMessages(EventMessageReceivedCallback callback)
        {
            CheckDisposed();
            CheckQueueDeclared();

            _logger.LogInformation("Start listening for messages on queue: {1}", QueueName);
            var consumer = _eventingBasicConsumerFactory.CreateEventingBasicConsumer(Channel);

            consumer.Received += (model, ea) =>
            {
                _logger.LogInformation("Received message with correlationId: {0}", ea.BasicProperties.CorrelationId);
                string body = Encoding.UTF8.GetString(ea.Body);

                callback.Invoke(new EventMessage(
                    routingKey: ea.RoutingKey,
                    message: body,
                    type: ea.BasicProperties.Type,
                    timestamp: ea.BasicProperties.Timestamp.UnixTime,
                    correlationId: ea.BasicProperties.CorrelationId
                ));

                _context.UpdateLastMessageReceived();
            };

            Channel.BasicConsume(
                queue: QueueName,
                autoAck: true,
                consumerTag: "",
                noLocal: false,
                exclusive: false,
                arguments: null,
                consumer: consumer
            );
        }

        private void CheckQueueAlreadyDeclared()
        {
            if (!_queueDeclared) return;
            _logger.LogDebug("Queue with name: {0} is already declared", QueueName);
            throw new BusConfigurationException($"Queue with name: {QueueName} is already declared");
        }

        private void CheckQueueDeclared()
        {
            if (_queueDeclared) return;
            _logger.LogDebug("Queue with name: {0} is not declared", QueueName);
            throw new BusConfigurationException($"Queue with name: {QueueName} is not declared");
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

        ~RabbitMQMessageReceiver()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Channel?.Dispose();
            }

            _disposed = true;
        }
    }
}
