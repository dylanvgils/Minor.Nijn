namespace Minor.Nijn.TestBus
{
    public interface IBusContextExtension : IBusContext<object>
    {
        ITestBuzz TestBus { get; }
    }
}