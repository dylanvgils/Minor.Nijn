namespace Minor.Nijn.TestBus
{
    public sealed class TestBusContextBuilder
    {
        private static readonly TestBusContext _instance;

        static TestBusContextBuilder()
        {
            var buzz = new TestBuzz();
            _instance = new TestBusContext(buzz);
        }

        public static TestBusContext CreateContext()
        {
            return _instance;
        }
    }
}
