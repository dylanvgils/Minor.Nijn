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
                host.RegisterListeners();
                Assert.IsTrue(foo.HasBeenInstantiated);
            }
        }

        [TestMethod]
        public void EventListerCanReceiveEventMessages()
        {
            var routingKey = TestClassesConstants.OrderEventHandlerTopic;
            var order = new Order {Id = 1, Description = "Some description"};
            var orderCreatedEvent = new OrderCreatedEvent(routingKey, order); 

            var busContext = new TestBusContextBuilder().CreateTestContext();
            var messageSender = busContext.CreateMessageSender();
            var hostBuilder = new MicroserviceHostBuilder()
                .WithContext(busContext)
                .AddListener<OrderEventListener>();

            using (var host = hostBuilder.CreateHost())
            {
                host.RegisterListeners();
                messageSender.SendMessage(new EventMessage(routingKey, JsonConvert.SerializeObject(orderCreatedEvent)));

                Assert.IsTrue(host.ListenersRegistered);
                Assert.IsTrue(OrderEventListener.HandleOrderCreatedEventHasBeenCalled);

                var result = OrderEventListener.HandleOrderCreatedEventHasBeenCalledWith;
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
                host.RegisterListeners();

                publisher.Publish(orderCreatedEvent);

                Assert.IsTrue(host.ListenersRegistered);
                Assert.IsTrue(OrderEventListener.HandleOrderCreatedEventHasBeenCalled);

                var result = OrderEventListener.HandleOrderCreatedEventHasBeenCalledWith;
                Assert.AreEqual(order.Id, result.Order.Id);
                Assert.AreEqual(order.Description, result.Order.Description);
            }
        }
    }
}
