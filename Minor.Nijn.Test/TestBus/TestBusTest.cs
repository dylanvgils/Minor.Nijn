using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using System.Collections.Generic;

namespace Minor.Nijn.Test.TestBus
{
    [TestClass]
    public class TestBusTest
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
            target.DeclareQueue("TestQueue", new List<string> { "nijn.TestBus.TestTopicAdded" });
            Assert.AreEqual(target.QueueLenght, 1);
        }

        [TestMethod]
        public void CreateMessageReceiver_WhenCalledTwiceQueueLenghtShouldBe_2()
        {
            target.DeclareQueue("TestQueue1", new List<string> { "nijn.TestBus.TestTopicAdded" });
            target.DeclareQueue("TestQueue2", new List<string> { "nijn.TestBus.TestTopicAdded" });
            Assert.AreEqual(target.QueueLenght, 2);
        }

        [TestMethod]
        public void CreateMessageReceiver_WhenCalledWithSameQueueNameTwiceLenghtShouldBe_1()
        {
            target.DeclareQueue("TestQueue1", new List<string> { "nijn.TestBus.TestTopicAdded" });
            target.DeclareQueue("TestQueue1", new List<string> { "nijn.TestBus.TestTopicAdded" });
            Assert.AreEqual(target.QueueLenght, 1);
        }
    }
}
