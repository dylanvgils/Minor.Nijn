using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Minor.Nijn.WebScale.Test.TestClasses;
using Minor.Nijn.WebScale.Test.TestClasses.Domain;
using Minor.Nijn.WebScale.Test.TestClasses.Events;
using Minor.Nijn.WebScale.Test.TestClasses.Injectable;
using Newtonsoft.Json;

namespace Minor.Nijn.WebScale.Events.Test
{
    [TestClass]
    public class EventIntegrationTest
    {
        [TestCleanup]
        public void AfterEach()
        {
            OrderEventListener.HandleOrderCreatedEventHasBeenCalled = false;
            OrderEventListener.HandleOrderCreatedEventHasBeenCalledWith = null;
        }

        [TestMethod]
        public void RegisteredDependenciesCanBeInjected()
        {
            var foo = new Foo();

            var eventMessage = new EventMessage(TestClassesConstants.ProductEventHandlerTopic, "message", "type");

            var busContext = new TestBusContextBuilder().CreateTestContext();
            var hostBuilder = new MicroserviceHostBuilder()
                .RegisterDependencies(services =>
                {
                    services.AddSingleton<IFoo>(foo);
                })
                .WithContext(busContext)
                .AddListener<ProductEventListener>();

            using (var host = hostBuilder.CreateHost())
            {
                host.StartListening();
                busContext.EventBus.DispatchMessage(eventMessage);

                Assert.IsTrue(foo.HasBeenInstantiated);
            }
        }

        [TestMethod]
        public void EventListerCanReceiveEventMessages()
        {
            var routingKey = TestClassesConstants.OrderEventHandlerTopic;
            var order = new Order {Id = 1, Description = "Some description"};
            var orderCreatedEvent = new OrderCreatedEvent(routingKey, order);
            var eventMessage = new EventMessage(
                routingKey: routingKey,
                message: JsonConvert.SerializeObject(orderCreatedEvent),
                type: orderCreatedEvent.GetType().Name,
                timestamp: orderCreatedEvent.Timestamp,
                correlationId: orderCreatedEvent.CorrelationId
            );

            var busContext = new TestBusContextBuilder().CreateTestContext();
            var messageSender = busContext.CreateMessageSender();
            var hostBuilder = new MicroserviceHostBuilder()
                .WithContext(busContext)
                .AddListener<OrderEventListener>();

            using (var host = hostBuilder.CreateHost())
            {
                host.StartListening();
                messageSender.SendMessage(eventMessage);

                Assert.IsTrue(OrderEventListener.HandleOrderCreatedEventHasBeenCalled);

                var result = OrderEventListener.HandleOrderCreatedEventHasBeenCalledWith;
                Assert.AreEqual(orderCreatedEvent.RoutingKey, result.RoutingKey);
                Assert.AreEqual(orderCreatedEvent.Timestamp, result.Timestamp);
                Assert.AreEqual(orderCreatedEvent.CorrelationId, result.CorrelationId);
                Assert.AreEqual(order.Id, result.Order.Id);
                Assert.AreEqual(order.Description, result.Order.Description);
            }
        }

        [TestMethod]
        public void EventPublisherCanSendEventMessage()
        {
            var routingKey = TestClassesConstants.OrderEventHandlerTopic;
            var order = new Order { Id = 1, Description = "Some description" };
            var orderCreatedEvent = new OrderCreatedEvent(routingKey, order);

            var busContext = new TestBusContextBuilder().CreateTestContext();
            var hostBuilder = new MicroserviceHostBuilder()
                .WithContext(busContext)
                .AddListener<OrderEventListener>();

            using (var host = hostBuilder.CreateHost())
            using(var publisher = new EventPublisher(busContext))
            {
                host.StartListening();

                publisher.Publish(orderCreatedEvent);

                Assert.IsTrue(OrderEventListener.HandleOrderCreatedEventHasBeenCalled);

                var result = OrderEventListener.HandleOrderCreatedEventHasBeenCalledWith;
                Assert.AreEqual(order.Id, result.Order.Id);
                Assert.AreEqual(order.Description, result.Order.Description);
            }
        }
    }
}
