using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Test.TestBus.Mocks;
using Minor.Nijn.TestBus;
using Moq;
using System.Collections.Generic;

namespace Minor.Nijn.Test.TestBus
{
    [TestClass]
    public class TestBusContextTest
    {
        private TestBusContext target;
        private Mock<ITestBuzz> mock;

        [TestInitialize]
        public void BeforeEach()
        {
            mock = new Mock<ITestBuzz>(MockBehavior.Strict);
            target = new TestBusContext(mock.Object);
        }

        [TestMethod]
        public void CreateMessageReceiver_ShouldReturnTestBusMessageReceiver()
        {
            string queueName = "TestQueue";
            IEnumerable<string> topicExpressions = new List<string> { "nijn.TestBus.TestTopic" };
            mock.Setup(buzz => buzz.DeclareQueue(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Returns(new TestBuzzQueue(topicExpressions));

            var result = target.CreateMessageReceiver(queueName, topicExpressions);

            mock.Verify(buzz => buzz.DeclareQueue(queueName, topicExpressions));
            Assert.IsInstanceOfType(result, typeof(IMessageReceiver));
            Assert.AreEqual(result.QueueName, queueName);
            Assert.AreEqual(result.TopicExpressions, topicExpressions);
        }
    }
}
