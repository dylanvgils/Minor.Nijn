using Minor.Nijn.RabbitMQBus;
using RabbitMQ.Client;

namespace Minor.Nijn.TestBus
{
    public sealed class TestBusContextBuilder
    {
        public IConnection Connection { get; private set; }
        public int ConnectionTimeoutAfterMs { get; private set; } = 500;

        public TestBusContextBuilder WithMockConnection(IConnection connection)
        {
            Connection = connection;
            return this;
        }

        public TestBusContextBuilder WithConnectionTimeout(int timeoutAfterMs)
        {
            ConnectionTimeoutAfterMs = timeoutAfterMs;
            return this;
        }

        public ITestBusContext CreateTestContext()
        {
            var eventBus = new EventBus.EventBus();
            var commandBus = new CommandBus.CommandBus();
            return new TestBusContext(Connection, eventBus, commandBus, ConnectionTimeoutAfterMs);
        }
    }
}
