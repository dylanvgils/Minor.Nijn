using System;

namespace Minor.Nijn.TestBus
{
    internal sealed class MessageAddedEventArgs : EventArgs
    {
        public EventMessage Message { get; }

        public MessageAddedEventArgs(EventMessage message)
        {
            Message = message;
        }
    }
}