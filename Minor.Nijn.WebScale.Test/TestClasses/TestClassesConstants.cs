namespace Minor.Nijn.WebScale.Test.TestClasses
{
    public class TestClassesConstants
    {
        // Audit event constants
        public const string AuditEventListenerQueueName = "EventBus.AuditEventQueue";
        public const string AuditEventListenerMethodName = "HandleEvents";

        // Order event constants
        public const string OrderEventListenerQueueName = "EventBus.OrderEventQueue";
        public const string OrderEventHandlerTopic = "OrderService.OrderCreated";
        public const string OrderEventHandlerMethodName = "HandleOrderCreatedEvent";

        // Order command constants
        public const string OrderCommandListenerQueueName = "CommandBus.AddedOrder";
        public const string OrderCommandHandlerMethodName = "HandleAddOrderCommand";

        // Product event constants
        public const string ProductEventListenerQueueName = "EventBus.ProductEventQueue";
        public const string ProductEventHandlerTopic = "ProductService.ProductAdded";
        public const string ProductEventHandlerMethodName = "HandleProductAddedEvent";

        // Product command constants
        public const string ProductCommandListenerQueueName = "CommandBus.AddProduct";
        public const string ProductCommandHandlerMethodName = "HandleAddProductCommand";
    }
}
