using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Nijn.WebScale.Events
{
    public class EventPublisher : IEventPublisher
    {
        private readonly IMessageSender _sender;

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
            _sender?.Dispose();
        }
    }
}
