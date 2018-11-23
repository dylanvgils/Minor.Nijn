using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Test.TestClasses.Events;

namespace Minor.Nijn.WebScale.Test.TestClasses
{
    [EventListener(TestClassesConstants.OrderEventListenerQueueName)]
    public class OrderEventListener
    {
        public static bool HandleOrderCreatedEventHasBeenCalled;
        public static OrderCreatedEvent HandleOrderCreatedEventHasBeenCalledWith;

        [Topic(TestClassesConstants.OrderEventHandlerTopic)]
        public void HandleOrderCreatedEvent(OrderCreatedEvent evt)
        {
            HandleOrderCreatedEventHasBeenCalled = true;
            HandleOrderCreatedEventHasBeenCalledWith = evt;
        }
    }
}