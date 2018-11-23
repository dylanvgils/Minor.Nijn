namespace Minor.Nijn.TestBus
{
    public sealed class TestBusContextBuilder
    {
        public TestBusContext CreateTestContext()
        {
            var eventBus = new EventBus.EventBus();
            var commandBus = new CommandBus.CommandBus();
            return new TestBusContext(eventBus, commandBus);
        }
    }
}
