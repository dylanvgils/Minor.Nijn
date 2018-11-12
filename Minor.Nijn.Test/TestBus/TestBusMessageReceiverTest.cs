using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Moq;
using System.Collections.Generic;

namespace Minor.Nijn.Test.TestBus
{
    [TestClass]
    public class TestBusMessageReceiverTest
    {
        private string queueName;
        private IEnumerable<string> topicExpressions;
        private Mock<IBusContextExtension> contextMock;
        private TestBusMessageReceiver target;

        [TestInitialize]
        public void BeforeEach()
        {
            queueName = "TestQueue";
            topicExpressions = new List<string> { "a.b.c" };

            contextMock = new Mock<IBusContextExtension>(MockBehavior.Strict);
            target = new TestBusMessageReceiver(contextMock.Object, queueName, topicExpressions);
        }

        [TestMethod]
        public void DeclareQueue_ShouldDeclareAQueueOnTheEventBuzz()
        {
            contextMock.Setup(context => context.TestBuzz.DeclareQueue(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(new TestBuzzQueue(topicExpressions));

            target.DeclareQueue();

            contextMock.Verify(context => context.TestBuzz.DeclareQueue(queueName, topicExpressions));
        }

        [TestMethod]
        public void AddMessage_ShouldRaiseMessageAddedEvent()
        {
            var callbackMock = new Mock<EventMessageReceivedCallback>(MockBehavior.Strict);
            callbackMock.Setup(callback => callback(It.IsAny<EventMessage>()));

            EventMessage message = new EventMessage("a.b.c", "TestMessage");
            var queue = new TestBuzzQueue(topicExpressions);
            contextMock.Setup(context => context.TestBuzz.DeclareQueue(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(queue);

            target.DeclareQueue();
            target.StartReceivingMessages(callbackMock.Object);
            queue.Enqueue(message);

            callbackMock.Verify(callback => callback(message));
        }
    }
}
