using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
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

            _log = NijnLogging.CreateLogger<RabbitMQMessageReceiver>();
            _eventingBasicConsumerFactory = new EventingBasicConsumerFactory();
        }

        public void DeclareQueue()
        {
            Channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            foreach (var topic in TopicExpressions)
            {
                Channel.QueueBind(
                    queue: QueueName,
                    exchange: _context.ExchangeName,
                    routingKey: topic, 
                    arguments: null
                );
            }    
        }

        public void StartReceivingMessages(EventMessageReceivedCallback Callback)
        {
            var consumer = _eventingBasicConsumerFactory.CreateEventingBasicConsumer(Channel);

            consumer.Received += (model, ea) =>
            {
                string body = Encoding.UTF8.GetString(ea.Body);

                _log.LogInformation($"Bericht ontvangen: {ea.Body}");

                Callback.Invoke(new EventMessage(
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

        public void Dispose()
        {
            Channel?.Dispose();
        }
    }
}
