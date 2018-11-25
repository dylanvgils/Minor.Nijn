using System;
using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Test.TestClasses.Commands;
using Minor.Nijn.WebScale.Test.TestClasses.Domain;

namespace Minor.Nijn.WebScale.Test.TestClasses
{
    [CommandListener]
    public class OrderCommandListener
    {
        public static Order ReplyWith;
        public static bool HandleOrderCreatedEventHasBeenCalled;
        public static AddOrderCommand HandleOrderCreatedEventHasBeenCalledWith;

        [Command(TestClassesConstants.OrderCommandListenerQueueName)]
        public Order HandleAddOrderCommand(AddOrderCommand command)
        {
            HandleOrderCreatedEventHasBeenCalled = true;
            HandleOrderCreatedEventHasBeenCalledWith = command;
            return ReplyWith;
        }
    }
}