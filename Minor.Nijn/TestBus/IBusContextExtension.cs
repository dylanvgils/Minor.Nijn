namespace Minor.Nijn.TestBus
{
    internal interface IBusContextExtension : IBusContext<object>
    {
        ITestBuzz TestBus { get; }
    }
}