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

        private RabbitMQMessageReceiver() { }
        
        internal RabbitMQMessageReceiver(IRabbitMQBusContext context, string queueName, IEnumerable<string> topicExpressions)
        {
            _context = context;
            QueueName = queueName;
            TopicExpressions = topicExpressions;
            Channel = _context.Connection.CreateModel();

            _log = NijnLogging.CreateLogger<RabbitMQMessageReceiver>();
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
            var consumer = new EventingBasicConsumer(Channel);

            consumer.Received += (model, ea) =>
            {
                string msg = Encoding.UTF8.GetString(ea.Body);

                _log.LogInformation($"Bericht ontvangen: {msg}");

                Callback.Invoke(new EventMessage(
                    routingKey: ea.RoutingKey,
                    message: msg,
                    eventType: null,
                    timestamp: 0,
                    correlationId: null
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
