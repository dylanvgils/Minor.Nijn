using Minor.Nijn.TestBus.CommandBus;
using Minor.Nijn.TestBus.EventBus;
using RabbitMQ.Client;

namespace Minor.Nijn.TestBus
{
    public interface ITestBusContext : IBusContext<IConnection>
    {
        IEventBus EventBus { get; }
        ICommandBus CommandBus { get; }
        int ConnectionTimeoutMs { get; }

        void UpdateLastMessageReceived();
    }
}