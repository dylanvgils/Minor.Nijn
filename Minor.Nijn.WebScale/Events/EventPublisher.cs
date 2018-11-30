using System;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Nijn.WebScale.Events
{
    public class EventPublisher : IEventPublisher
    {
        private readonly IMessageSender _sender;
        private bool _disposed;

        public EventPublisher(IBusContext<IConnection> context)
        {
            _sender = context.CreateMessageSender();
        }

        public void Publish(DomainEvent domainEvent)
        {
            CheckDisposed();

            var body = JsonConvert.SerializeObject(domainEvent);

            var message = new EventMessage(
                routingKey: domainEvent.RoutingKey,
                message: body,
                type: domainEvent.GetType().Name,
                timestamp: domainEvent.Timestamp,
                correlationId: domainEvent.CorrelationId
            );

            _sender.SendMessage(message);
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

        ~EventPublisher()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _sender?.Dispose();
            }

            _disposed = true;
        }
    }
}
