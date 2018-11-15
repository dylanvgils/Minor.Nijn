using RabbitMQ.Client;

namespace Minor.Nijn.RabbitMQBus
{
    public interface IRabbitMQBusContext : IBusContext<IConnection>
    {
    }
}