using System;

namespace Minor.Nijn.TestBus
{
    public sealed class MessageAddedEventArgs : EventArgs
    {
        public EventMessage Message { get; }

        public MessageAddedEventArgs(EventMessage message)
        {
            Message = message;
        }
    }
}