using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQMessageReceiver : IMessageReceiver
    {
        private readonly ILogger _log;
        
        public string QueueName { get; }
        public IEnumerable<string> TopicExpressions { get; }
        public IModel Channel { get; }
        private bool _queueDeclared;
        
        private readonly IRabbitMQBusContext _context;
        private readonly EventingBasicConsumerFactory _eventingBasicConsumerFactory;

        private RabbitMQMessageReceiver() { }

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
            
            _log = NijnLogger.CreateLogger<RabbitMQMessageReceiver>();
        }

        public void DeclareQueue()
        {
            _log.LogInformation("Declare queue on exchange with name: {0}", QueueName);
            
            Channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            foreach (var topic in TopicExpressions)
            {
                _log.LogInformation("Bind queue {0} to exchange {1} with with topic {2}", QueueName, _context.ExchangeName, topic);
                
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
            CheckQueueDeclared();

            _log.LogInformation("Start listening for messages on queue: {1}", QueueName);
            var consumer = _eventingBasicConsumerFactory.CreateEventingBasicConsumer(Channel);

            consumer.Received += (model, ea) =>
            {
                _log.LogInformation("Received message {0}", ea.BasicProperties.MessageId);
                string body = Encoding.UTF8.GetString(ea.Body);

                callback.Invoke(new EventMessage(
                    routingKey: ea.RoutingKey,
                    message: body,
                    eventType: ea.BasicProperties.Type,
                    timestamp: ea.BasicProperties.Timestamp.UnixTime,
                    correlationId: ea.BasicProperties.CorrelationId
                ));
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

        private void CheckQueueDeclared()
        {
            if (!_queueDeclared)
            {
                throw new BusConfigurationException($"Queue with name: {QueueName} is not declared");
            }
        }

        public void Dispose()
        {
            _log.LogInformation("Disposing message receiver for queue: {0}", QueueName);
            Channel?.Dispose();
        }
    }
}
