using System;

namespace Minor.Nijn.TestBus
{
    public sealed class TestBusContextBuilder
    {
        public string CommandQueueName { get; private set; } = "RpcQueue";

        public TestBusContextBuilder WithCommandQueue(string queueName)
        {
            CommandQueueName = queueName;
            return this;
        }

        public TestBusContext CreateTestContext()
        {
            var eventBus = new EventBus.EventBus();
            var commandBus = new CommandBus.CommandBus();
            return new TestBusContext(eventBus, commandBus, CommandQueueName);
        }
    }
}
