namespace Minor.Nijn.TestBus
{
    internal interface IBus<T>
    {
        void DispatchMessage(T message);
    }
}