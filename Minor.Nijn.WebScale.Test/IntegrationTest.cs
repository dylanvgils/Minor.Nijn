using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Minor.Nijn.WebScale.Test.TestClasses;
using Minor.Nijn.WebScale.Test.TestClasses.Domain;
using Minor.Nijn.WebScale.Test.TestClasses.Events;
using Newtonsoft.Json;

namespace Minor.Nijn.WebScale.Test
{
    [TestClass]
    public class IntegrationTest
    {
        [TestCleanup]
        public void AfterEach()
        {
            OrderEventListener.HandleOrderCreatedEventHasBeenCalled = false;
            OrderEventListener.HandleOrderCreatedEventHasBeenCalledWith = null;
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
                .AddEventListener<OrderEventListener>();

            var host = hostBuilder.CreateHost();
            messageSender.SendMessage(new EventMessage(routingKey, JsonConvert.SerializeObject(orderCreatedEvent)));

            Assert.IsTrue(host.EventListenersRegistered);
            Assert.IsTrue(OrderEventListener.HandleOrderCreatedEventHasBeenCalled);

            var result = OrderEventListener.HandleOrderCreatedEventHasBeenCalledWith;
            Assert.AreEqual(order.Id, result.Order.Id);
            Assert.AreEqual(order.Description, result.Order.Description);
        }
    }
}
