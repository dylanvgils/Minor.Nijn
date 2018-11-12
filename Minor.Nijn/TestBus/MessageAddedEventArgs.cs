using System;

namespace Minor.Nijn.TestBus
{
    public class MessageAddedEventArgs : EventArgs
    {
        public EventMessage Message { get; }

        public MessageAddedEventArgs(EventMessage message)
        {
            Message = message;
        }
    }
}