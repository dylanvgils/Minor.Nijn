using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using System.Collections.Generic;

namespace Minor.Nijn.Test.TestBus
{
    [TestClass]
    public class TestBusContextTest
    {
        private TestBusContext target;

        [TestInitialize]
        public void BeforeEach()
        {
            target = new TestBusContext();
        }

        [TestMethod]
        public void CreateMessageReceiver_ShouldReturnTestBusMessageReceiver()
        {
            string queueName = "TestQueue";
            IEnumerable<string> topicExpressions = new List<string>
            {
                "nijn.TestBus.TestTopic",
            };

            var result = target.CreateMessageReceiver(queueName, topicExpressions);

            Assert.IsInstanceOfType(result, typeof(IMessageReceiver));
            Assert.AreEqual(result.QueueName, queueName);
            Assert.AreEqual(result.TopicExpressions, topicExpressions);
        }

        [TestMethod]
        public void CreateMessageReceiver_QueueLengtShouldBe_1()
        {
            target.CreateMessageReceiver("TestQueue", new List<string> { "nijn.TestBus.TestTopic" });
            Assert.AreEqual(target.QueueLenght, 1);
        }

        [TestMethod]
        public void CreateMessageReceiver_WhenCalledTwiceQueueLenghtShouldBe_2()
        {
            target.CreateMessageReceiver("TestQueue1", new List<string> { "nijn.TestBus.TestTopic" });
            target.CreateMessageReceiver("TestQueue2", new List<string> { "nijn.TestBus.TestTopic" });
            Assert.AreEqual(target.QueueLenght, 2);
        }

        [TestMethod]
        public void CreateMessageReceiver_WhenCalledWithSameQueueNameTwiceLenghtShouldBe_1()
        {
            target.CreateMessageReceiver("TestQueue1", new List<string> { "nijn.TestBus.TestTopic" });
            target.CreateMessageReceiver("TestQueue1", new List<string> { "nijn.TestBus.TestTopic" });
            Assert.AreEqual(target.QueueLenght, 1);
        }
    }
}
