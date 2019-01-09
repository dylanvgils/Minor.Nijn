using RabbitMQ.Client;

namespace Minor.Nijn.RabbitMQBus
{
    public interface IRabbitMQBusContext : IBusContext<IConnection>
    {
        bool AutoDisconnectEnabled { get; }
        int ConnectionTimeoutMs { get; }

        void UpdateLastMessageReceived();
    }
}