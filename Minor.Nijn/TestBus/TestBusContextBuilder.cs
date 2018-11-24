namespace Minor.Nijn.TestBus
{
    public sealed class TestBusContextBuilder
    {
        public ITestBusContext CreateTestContext()
        {
            var eventBus = new EventBus.EventBus();
            var commandBus = new CommandBus.CommandBus();
            return new TestBusContext(eventBus, commandBus);
        }
    }
}
