using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Test.TestBus.Mock;
using Minor.Nijn.TestBus;
using System.Collections.Generic;

namespace Minor.Nijn.Test.TestBus
{
    [TestClass]
    public class TestBuzzQueueTest
    {
        private TestBuzzQueue target;

        [TestInitialize]
        public void BeforeEach()
        {
            IEnumerable<string> topicExpressions = new List<string> { "a.b.c" };
            target = new TestBuzzQueue(topicExpressions);
        }

        [TestMethod]
        public void Enqueue_ShouldRaiseMessageAddedEventWhenMessageAdded()
        {
            var mock = new MessageAddedMock();
            var message = new EventMessage("a.b.c", "Test message.");
            target.MessageAdded += mock.HandleMessageAdded;

            target.Enqueue(message);

            Assert.IsTrue(mock.HandledMessageAddedHasBeenCalled);
            Assert.AreEqual(message, mock.Args.Message);
        }

        [TestMethod]
        public void Enqueue_ShouldNotRaiseEventWhenRoutingKeyNotExists()
        {
            var mock = new MessageAddedMock();
            var message = new EventMessage("a.b.d", "Test message.");
            target.MessageAdded += mock.HandleMessageAdded;

            target.Enqueue(message);

            Assert.IsFalse(mock.HandledMessageAddedHasBeenCalled);
        }
    }
}
