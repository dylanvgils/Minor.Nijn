namespace Minor.Nijn.TestBus
{
    public interface ITestBus<T>
    {
        int QueueCount { get; }

        void DispatchMessage(T message);
    }
}