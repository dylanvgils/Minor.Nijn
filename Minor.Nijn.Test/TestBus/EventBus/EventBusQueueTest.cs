using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Minor.Nijn.TestBus.Mocks.Test;

namespace Minor.Nijn.TestBus.EventBus.Test
{
    [TestClass]
    public class EventBusQueueTest
    {
        private EventBusQueue _target;

        [TestInitialize]
        public void BeforeEach()
        {
            string queueName = "TestQueue";
            IEnumerable<string> topicExpressions = new List<string> { "a.b.c" };
            _target = new EventBusQueue(queueName, topicExpressions);
        }

        [TestMethod]
        public void Subscribe_ShouldSubscribeToTheMessageAddedEvent()
        {
            var mock = new MessageAddedMock<EventMessage>();
            _target.Subscribe(mock.HandleMessageAdded);
            Assert.AreEqual(1, _target.SubscriberCount);
        }

        [TestMethod]
        public void Subscribe_ShouldSubscribeMultipleEvents()
        {
            var mock1 = new MessageAddedMock<EventMessage>();
            var mock2 = new MessageAddedMock<EventMessage>();

            _target.Subscribe(mock1.HandleMessageAdded);
            _target.Subscribe(mock2.HandleMessageAdded);

            Assert.AreEqual(2, _target.SubscriberCount);
        }

        [TestMethod]
        public void Unsubscribe_ShouldUnsubscribeEvent()
        {
            var mock1 = new MessageAddedMock<EventMessage>();
            var mock2 = new MessageAddedMock<EventMessage>();
            _target.Subscribe(mock1.HandleMessageAdded);
            _target.Subscribe(mock2.HandleMessageAdded);

            _target.Unsubscribe(mock1.HandleMessageAdded);

            Assert.AreEqual(1, _target.SubscriberCount);
        }

        [TestMethod]
        public void Enqueue_ShouldQueueMessagesWhenNoEventHandlersAreSubscribed()
        {
            var message1 = new EventMessage("a.b.c", "Test message 1");
            var message2 = new EventMessage("a.b.c", "Test message 2");

            _target.Enqueue(message1);
            _target.Enqueue(message2);

            Assert.AreEqual(2, _target.MessageQueueLength);
        }

        [TestMethod]
        public void Subscribe_ShouldDequeueAllExistingMessagesInMessageQueue()
        {
            var message1 = new EventMessage("a.b.c", "Test message 1");
            var message2 = new EventMessage("a.b.c", "Test message 2");
            _target.Enqueue(message1);
            _target.Enqueue(message2);

            var mock = new MessageAddedMock<EventMessage>();
            _target.Subscribe(mock.HandleMessageAdded);

            Assert.IsTrue(mock.HandledMessageAddedHasBeenCalled);
            Assert.AreEqual(2, mock.HandleMessageAddedCount);
            Assert.AreEqual(0, _target.MessageQueueLength);
        }

        [TestMethod]
        public void Enqueue_ShouldRaiseMessageAddedEventWhenMessageAdded()
        {
            var mock = new MessageAddedMock<EventMessage>();
            var message = new EventMessage("a.b.c", "Test message.");
            _target.Subscribe(mock.HandleMessageAdded);

            _target.Enqueue(message);

            Assert.IsTrue(mock.HandledMessageAddedHasBeenCalled);
            Assert.AreEqual(message, mock.Args.Message);
        }

        [TestMethod]
        public void Enqueue_ShouldNotRaiseEventWhenRoutingKeyNotExists()
        {
            var mock = new MessageAddedMock<EventMessage>();
            var message = new EventMessage("a.b.d", "Test message.");
            _target.Subscribe(mock.HandleMessageAdded);

            _target.Enqueue(message);

            Assert.IsFalse(mock.HandledMessageAddedHasBeenCalled);
        }

        [TestMethod]
        public void Indexer_ShouldReturnItemAtIndex()
        {
            var msg1 = new EventMessage("a.b.c", "Message1");
            var msg2 = new EventMessage("a.b.c", "Message2");
            var msg3 = new EventMessage("a.b.c", "Message3");
   
            _target.Enqueue(msg1);
            _target.Enqueue(msg2);
            _target.Enqueue(msg3);

            Assert.AreEqual(msg1, _target[0]);
            Assert.AreEqual(msg2, _target[1]);
            Assert.AreEqual(msg3, _target[2]);
        }
    }
}
