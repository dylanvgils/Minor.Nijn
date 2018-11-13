using System;

namespace Minor.Nijn.TestBus
{
    internal sealed class MessageAddedEventArgs<T> : EventArgs
    {
        public T Message { get; }

        public MessageAddedEventArgs(T message)
        {
            Message = message;
        }
    }
}