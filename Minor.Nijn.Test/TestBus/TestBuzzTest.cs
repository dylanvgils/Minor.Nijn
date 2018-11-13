using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Test.TestBus.Mock;
using Minor.Nijn.TestBus;
using System.Collections.Generic;

namespace Minor.Nijn.Test.TestBus
{
    [TestClass]
    public class TestBuzzTest
    {
        private TestBuzz target;
        [TestInitialize]
        public void BeforeEach()
        {
            target = new TestBuzz();
        }

        [TestMethod]
        public void CreateMessageReceiver_QueueLengtShouldBe_1()
        {
            target.DeclareQueue("TestQueue", new List<string> { "a.b.c" });
            Assert.AreEqual(target.QueueLength, 1);
        }

        [TestMethod]
        public void CreateMessageReceiver_WhenCalledTwiceQueueLenghtShouldBe_2()
        {
            target.DeclareQueue("TestQueue1", new List<string> { "a.b.c" });
            target.DeclareQueue("TestQueue2", new List<string> { "a.b.c" });
            Assert.AreEqual(target.QueueLength, 2);
        }

        [TestMethod]
        public void CreateMessageReceiver_WhenCalledWithSameQueueNameTwiceLenghtShouldBe_1()
        {
            target.DeclareQueue("TestQueue1", new List<string> { "a.b.c" });
            target.DeclareQueue("TestQueue1", new List<string> { "a.b.c" });
            Assert.AreEqual(target.QueueLength, 1);
        }

        [TestMethod]
        public void DispatchMessage_ShouldTriggerEvent()
        {
            var mock = new MessageAddedMock();
            var message = new EventMessage("a.b.c", "Test message");
            var queue = target.DeclareQueue("TestQueue1", new List<string> { "a.b.c" });
            queue.MessageAdded += mock.HandleMessageAdded;

            target.DispatchMessage(message);

            Assert.IsTrue(mock.HandledMessageAddedHasBeenCalled);
            Assert.AreEqual(message, mock.Args.Message);
        }

        [TestMethod]
        public void DispatchMessage_ShouldTrigger_2_EventsWhen_2_QueuesAreDeclared()
        {
            var message = new EventMessage("a.b.c", "Test message");

            var mock1 = new MessageAddedMock();
            var queue1 = target.DeclareQueue("TestQueue1", new List<string> { "a.b.c" });
            queue1.MessageAdded += mock1.HandleMessageAdded;

            var mock2 = new MessageAddedMock();
            var queue2 = target.DeclareQueue("TestQueue2", new List<string> { "a.b.c" });
            queue1.MessageAdded += mock2.HandleMessageAdded;

            target.DispatchMessage(message);

            Assert.IsTrue(mock1.HandledMessageAddedHasBeenCalled);
            Assert.AreEqual(message, mock1.Args.Message);

            Assert.IsTrue(mock2.HandledMessageAddedHasBeenCalled);
            Assert.AreEqual(message, mock2.Args.Message);
        }
    }
}
