using Minor.Nijn.TestBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.Test.TestBus.Mock
{
    internal class MessageAddedMock
    {
        public bool HandledMessageAddedHasBeenCalled;
        public MessageAddedEventArgs Args;

        public void HandleMessageAdded(object sender, MessageAddedEventArgs args)
        {
            HandledMessageAddedHasBeenCalled = true;
            Args = args;
        }
    }
}
