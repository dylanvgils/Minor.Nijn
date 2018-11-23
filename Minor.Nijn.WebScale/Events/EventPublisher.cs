using RabbitMQ.Client;
using System;

namespace Minor.Nijn.WebScale.Events
{
    public class EventPublisher : IEventPublisher
    {
        IBusContext<IConnection> Context { get; set; }

        public EventPublisher(IBusContext<IConnection> context)
        {
            context = Context;
        }

        public void Publish(DomainEvent domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
