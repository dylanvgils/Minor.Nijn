using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQMessageSender : IMessageSender
    {
        private readonly ILogger _logger;
        private bool _disposed;

        public IModel Channel { get; }
        private readonly IRabbitMQBusContext _context;

        internal RabbitMQMessageSender(IRabbitMQBusContext context)
        {
            _context = context;
            Channel = context.Connection.CreateModel();

            _logger = NijnLogger.CreateLogger<RabbitMQMessageSender>();
        }

        public void SendMessage(EventMessage message)
        {
            CheckDisposed();
            _logger.LogInformation("Send message to routing key: {0}", message.RoutingKey);

            var props = Channel.CreateBasicProperties();
            props.Type = message.Type ?? "";
            props.CorrelationId = message?.CorrelationId ?? Guid.NewGuid().ToString();
            props.Timestamp = message.Timestamp == 0 
                ? new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) 
                : new AmqpTimestamp(message.Timestamp);

            Channel.BasicPublish(
                exchange: _context.ExchangeName,
                routingKey: message.RoutingKey,
                mandatory: false,
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(message.Message)
            );
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

        ~RabbitMQMessageSender()
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
