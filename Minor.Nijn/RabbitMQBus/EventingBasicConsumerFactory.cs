using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Minor.Nijn.RabbitMQBus
{
    internal class EventingBasicConsumerFactory
    {
        public virtual EventingBasicConsumer CreateEventingBasicConsumer(IModel channel)
        {
            return new EventingBasicConsumer(channel);
        }
    }
}