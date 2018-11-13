using Minor.Nijn.TestBus.CommandBus;
using Minor.Nijn.TestBus.EventBus;

namespace Minor.Nijn.TestBus
{
    public sealed class TestBusContextBuilder
    {
        private static readonly TestBusContext _instance;

        static TestBusContextBuilder()
        {
            var eventBus = new EventBus.EventBus();
            var commandBus = new CommandBus.CommandBus();
            _instance = new TestBusContext(eventBus, commandBus);
        }

        public static TestBusContext CreateContext()
        {
            return _instance;
        }
    }
}
