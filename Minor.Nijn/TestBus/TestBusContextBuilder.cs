namespace Minor.Nijn.TestBus
{
    public sealed class TestBusContextBuilder
    {
        private static readonly TestBusContext _instance = new TestBusContext();

        public static TestBusContext CreateContext()
        {
            return _instance;
        }
    }
}
