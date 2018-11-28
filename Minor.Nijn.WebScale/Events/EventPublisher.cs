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
            var body = JsonConvert.SerializeObject(domainEvent);
            var message = new EventMessage(domainEvent.RoutingKey, body);
            _sender.SendMessage(message);
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
