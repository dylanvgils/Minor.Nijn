using Microsoft.Extensions.Logging;
using Minor.Nijn.Audit.Entities;
using RabbitMQ.Client;
using System;
using System.Text;

namespace Minor.Nijn.Audit
{
    internal class EventReplayer : IEventReplayer, IDisposable
    {
        private readonly ILogger _logger;

        public string ExchangeName { get; private set; }
        public bool ExchangeDeclared { get; private set; }

        private readonly IModel _channel;
        private bool _disposed;

        public EventReplayer(IBusContext<IConnection> busContext, ILoggerFactory logger)
        {
            _channel = busContext.Connection.CreateModel();
            _logger = logger.CreateLogger<EventReplayer>();
        }

        public void DeclareExchange(string exchangeName)
        {
            if (ExchangeDeclared)
            {
                _logger.LogError("Exchange with name: {1} already declared", exchangeName);
                throw new InvalidOperationException($"Exchange with name: {exchangeName} already declared");
            }

            _logger.LogInformation("Declaring exchange with name: {0}", exchangeName);

            _channel.ExchangeDeclare(
                exchange: exchangeName,
                type: Constants.ReplayerExchangeType,
                durable: false,
                autoDelete: true,
                arguments: null
            );

            ExchangeName = exchangeName;
            ExchangeDeclared = true;
        }

        public void ReplayAuditMessage(AuditMessage message)
        {
            CheckDisposed();
            if (!ExchangeDeclared)
            {
                _logger.LogError("Exchange should be declared");
                throw new InvalidOperationException("Exchange should be declared");
            }

            var props = _channel.CreateBasicProperties();
            props.CorrelationId = message.CorrelationId ?? Guid.NewGuid().ToString();
            props.Timestamp = new AmqpTimestamp(message.Timestamp);
            props.Type = message.Type;

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: message.RoutingKey,
                mandatory: false,
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(message.Payload)
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

        ~EventReplayer()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _channel?.Dispose();
            }

            _disposed = true;
        }
    }
}