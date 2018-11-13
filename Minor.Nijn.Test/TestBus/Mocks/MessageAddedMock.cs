using Minor.Nijn.TestBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.Test.TestBus.Mock
{
    internal class MessageAddedMock<T>
    {
        public bool HandledMessageAddedHasBeenCalled;
        public MessageAddedEventArgs<T> Args;

        public void HandleMessageAdded(object sender, MessageAddedEventArgs<T> args)
        {
            HandledMessageAddedHasBeenCalled = true;
            Args = args;
        }
    }
}
