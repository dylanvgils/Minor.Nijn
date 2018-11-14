using Minor.Nijn.TestBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.TestBus.Mocks.Test
{
    internal class MessageAddedMock<T>
    {
        public bool HandledMessageAddedHasBeenCalled;
        public int HandleMessageAddedCount;
        public MessageAddedEventArgs<T> Args;

        public void HandleMessageAdded(object sender, MessageAddedEventArgs<T> args)
        {
            HandledMessageAddedHasBeenCalled = true;
            HandleMessageAddedCount++;
            Args = args;
        }
    }
}
