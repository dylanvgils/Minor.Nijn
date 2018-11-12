using System;

namespace Minor.Nijn.TestBus
{
    public class MessageAddedEventArgs : EventArgs
    {
        public EventMessage Message { get; private set; }

        public MessageAddedEventArgs(EventMessage message)
        {
            Message = message;
        }
    }
}