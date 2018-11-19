using System;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQMessageSender : IMessageSender
    {
        private readonly ILogger _log;

        public IModel Channel { get; }
        private readonly IRabbitMQBusContext _context;

        private RabbitMQMessageSender() { }
        
        internal RabbitMQMessageSender(IRabbitMQBusContext context)
        {
            _context = context;
            Channel = context.Connection.CreateModel();

            _log = NijnLogging.CreateLogger<RabbitMQMessageSender>();
        }

        public void SendMessage(EventMessage message)
        {
            _log.LogInformation("Send message to routing key: {0}", message.RoutingKey);

            var props = Channel.CreateBasicProperties();
            props.Type = message.EventType ?? "";
            props.CorrelationId = message?.CorrelationId ?? "";
            props.Timestamp = message.Timestamp == 0 
                ? new AmqpTimestamp(DateTime.Now.Ticks) 
                : new AmqpTimestamp(message.Timestamp);

            Channel.BasicPublish(
                exchange: _context.ExchangeName,
                routingKey: message.RoutingKey,
                mandatory: false,
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(message.Message)
            );
        }

        public void Dispose()
        {
            Channel?.Dispose();
        }
    }
}
