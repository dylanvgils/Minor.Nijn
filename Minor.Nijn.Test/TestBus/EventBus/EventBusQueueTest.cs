using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Test.TestBus.Mock;
using System.Collections.Generic;
using Minor.Nijn.TestBus.EventBus;

namespace Minor.Nijn.Test.TestBus
{
    [TestClass]
    public class EventBusQueueTest
    {
        private EventBusQueue target;

        [TestInitialize]
        public void BeforeEach()
        {
            string queueName = "TestQueue";
            IEnumerable<string> topicExpressions = new List<string> { "a.b.c" };
            target = new EventBusQueue(queueName, topicExpressions);
        }

        [TestMethod]
        public void Enqueue_ShouldRaiseMessageAddedEventWhenMessageAdded()
        {
            var mock = new MessageAddedMock<EventMessage>();
            var message = new EventMessage("a.b.c", "Test message.");
            target.MessageAdded += mock.HandleMessageAdded;

            target.Enqueue(message);

            Assert.IsTrue(mock.HandledMessageAddedHasBeenCalled);
            Assert.AreEqual(message, mock.Args.Message);
        }

        [TestMethod]
        public void Enqueue_ShouldNotRaiseEventWhenRoutingKeyNotExists()
        {
            var mock = new MessageAddedMock<EventMessage>();
            var message = new EventMessage("a.b.d", "Test message.");
            target.MessageAdded += mock.HandleMessageAdded;

            target.Enqueue(message);

            Assert.IsFalse(mock.HandledMessageAddedHasBeenCalled);
        }
    }
}
