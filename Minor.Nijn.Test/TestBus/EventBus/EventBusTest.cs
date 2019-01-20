using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Minor.Nijn.TestBus.Mocks.Test;

namespace Minor.Nijn.TestBus.EventBus.Test
{
    [TestClass]
    public class EventBusTest
    {
        private EventBus target;

        [TestInitialize]
        public void BeforeEach()
        {
            target = new EventBus();
        }

        [TestMethod]
        public void DispatchMessage_ShouldTriggerEvent()
        {
            var mock = new MessageAddedMock<EventMessage>();
            var message = new EventMessage("a.b.c", "Test message");
            var queue = target.DeclareQueue("TestQueue1", new List<string> { "a.b.c" });
            queue.Subscribe(mock.HandleMessageAdded);

            target.DispatchMessage(message);

            Assert.IsTrue(mock.HandledMessageAddedHasBeenCalled);
            Assert.AreEqual(1, queue.CalledTimes);
            Assert.AreEqual(message, mock.Args.Message);
        }

        [TestMethod]
        public void DispatchMessage_ShouldTrigger_2_EventsWhen_2_QueuesAreDeclared()
        {
            var message = new EventMessage("a.b.c", "Test message");

            var mock1 = new MessageAddedMock<EventMessage>();
            var queue1 = target.DeclareQueue("TestQueue1", new List<string> { "a.b.c" });
            queue1.Subscribe(mock1.HandleMessageAdded);

            var mock2 = new MessageAddedMock<EventMessage>();
            var queue2 = target.DeclareQueue("TestQueue2", new List<string> { "a.b.c" });
            queue2.Subscribe(mock2.HandleMessageAdded);

            target.DispatchMessage(message);

            Assert.IsTrue(mock1.HandledMessageAddedHasBeenCalled);
            Assert.AreEqual(message, mock1.Args.Message);

            Assert.IsTrue(mock2.HandledMessageAddedHasBeenCalled);
            Assert.AreEqual(message, mock2.Args.Message);
        }
        
        [TestMethod]
        public void DeclareQueue_QueueLengthShouldBe_1()
        {
            string queueName = "TestQueue";
            IEnumerable<string> topicExpressions  = new List<string> { "a.b.c" };
            
            var result = target.DeclareQueue(queueName, topicExpressions);
            
            Assert.IsInstanceOfType(result, typeof(EventBusQueue));
            Assert.AreEqual(queueName, result.QueueName);
            Assert.AreEqual(topicExpressions, result.TopicExpressions);
            Assert.AreEqual(target.QueueCount, 1);
        }

        [TestMethod]
        public void DeclareQueue_WhenCalledTwiceQueueLengthShouldBe_2()
        {
            target.DeclareQueue("TestQueue1", new List<string> { "a.b.c" });
            target.DeclareQueue("TestQueue2", new List<string> { "a.b.c" });
            Assert.AreEqual(target.QueueCount, 2);
        }

        [TestMethod]
        public void DeclareQueue_WhenCalledWithSameQueueNameTwiceLengthShouldBe_1()
        {
            var queueName = "TestQueue1";
            var topic1 = "a.b.c";
            var topic2 = "x.y.z";

            target.DeclareQueue(queueName, new List<string> { topic1 });
            target.DeclareQueue(queueName, new List<string> { topic2 });

            Assert.AreEqual(target.QueueCount, 1);
            Assert.IsTrue(target.Queues[queueName].TopicExpressions.Contains(topic1), "Should contain topic1");
            Assert.IsTrue(target.Queues[queueName].TopicExpressions.Contains(topic2), "should contain topic2");
        }
    }
}
