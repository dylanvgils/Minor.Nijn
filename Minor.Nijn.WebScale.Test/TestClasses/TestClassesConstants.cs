namespace Minor.Nijn.WebScale.Test.TestClasses
{
    public class TestClassesConstants
    {
        // Order constants
        public const string OrderEventListenerQueueName = "EventBus.OrderEventQueue";
        public const string OrderEventHandlerTopic = "OrderService.OrderCreated";
        public const string OrderEventHandlerMethodName = "HandleOrderCreatedEvent";

        // Product constants
        public const string ProductEventListenerQueueName = "EventBus.ProductEventQueue";
        public const string ProductEventHandlerTopic = "ProductService.ProductAdded";
        public const string ProductEventHandlerMethodName = "HandleProductAddedEvent";
    }
}
