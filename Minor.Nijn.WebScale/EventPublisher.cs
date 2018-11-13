using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.WebScale
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
