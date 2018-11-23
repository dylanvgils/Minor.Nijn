using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.WebScale.Test.TestClasses;
using Minor.Nijn.WebScale.Test.TestClasses.Domain;
using Minor.Nijn.WebScale.Test.TestClasses.Events;
using Newtonsoft.Json;

namespace Minor.Nijn.WebScale.Test
{
    [TestClass]
    public class EventListenerTest
    {
        private EventListener target;

        [TestInitialize]
        public void BeforeEach()
        {
            var type = typeof(OrderEventListener);
            var method = type.GetMethod(TestClassesConstants.OrderEventHandlerMethodName);
            var queueName = "queueName";
            var topicExpressions = new List<string> { "a.b.c" };

            target = new EventListener(type, method, queueName, topicExpressions);
        }

        [TestCleanup]
        public void AfterEach()
        {
            OrderEventListener.HandleOrderCreatedEventHasBeenCalled = false;
            OrderEventListener.HandleOrderCreatedEventHasBeenCalledWith = null;
        }

        [TestMethod]
        public void Ctor_ShouldCreatedNewEventListener()
        {
            var type = typeof(OrderEventListener);
            var method = type.GetMethod(TestClassesConstants.OrderEventHandlerMethodName);
            var queueName = "queueName";
            var topicExpressions = new List<string> {"a.b.c"};

            var listener = new EventListener(type, method, queueName, topicExpressions);

            Assert.AreEqual(queueName, listener.QueueName);
            Assert.AreEqual(topicExpressions, listener.TopicPatterns);
        }

        [TestMethod]
        public void HandleEventMessage_ShouldHandleEventMessageAndReturnCastedType()
        {
            var routingKey = "a.b.c";
            var order = new Order { Id = 1, Description = "Some Description" };
            var orderCreatedEvent = new OrderCreatedEvent(routingKey, order);
            var eventMessage = new EventMessage(routingKey, JsonConvert.SerializeObject(orderCreatedEvent));

            target.HandleEventMessage(eventMessage);

            var result = OrderEventListener.HandleOrderCreatedEventHasBeenCalledWith;
            Assert.IsTrue(OrderEventListener.HandleOrderCreatedEventHasBeenCalled);
            Assert.AreEqual(order.Id, result.Order.Id);
            Assert.AreEqual(order.Description, result.Order.Description);
        }
    }
}
