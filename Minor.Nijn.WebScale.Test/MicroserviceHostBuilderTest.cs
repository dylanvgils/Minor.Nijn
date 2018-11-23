using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.WebScale.Test.TestClasses;
using Moq;
using RabbitMQ.Client;

namespace Minor.Nijn.WebScale.Test
{
    [TestClass]
    public class MicroserviceHostBuilderTest
    {
        private Mock<IMessageReceiver> receiverMock;
        private Mock<IBusContext<IConnection>> busContextMock;

        private MicroserviceHostBuilder target;

        [TestInitialize]
        public void BeforeEach()
        {
            receiverMock = new Mock<IMessageReceiver>(MockBehavior.Strict);
            receiverMock.Setup(recv => recv.DeclareQueue());
            receiverMock.Setup(recv => recv.StartReceivingMessages(It.IsAny<EventMessageReceivedCallback>()));

            busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            busContextMock.Setup(ctx => ctx.CreateMessageReceiver(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(receiverMock.Object);

            target = new MicroserviceHostBuilder();
        }

        [TestMethod]
        public void CreateHost_ShouldReturnMicroserviceHost()
        {
            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            var result = target.WithContext(busContextMock.Object).CreateHost();
            Assert.IsInstanceOfType(result, typeof(MicroserviceHost));
        }

        [TestMethod]
        public void CreateHost_ShouldReturnMicroserviceHostWithOneEventListenerWhenCalledWithAddEventListener()
        {
            var result = target.AddEventListener<ProductEventListener>().WithContext(busContextMock.Object).CreateHost();

            var listener = result.EventListeners.First();
            Assert.AreEqual(1, result.EventListeners.Count());
            Assert.AreEqual(TestClassesConstants.ProductEventListenerQueueName, listener.QueueName);
            Assert.AreEqual(TestClassesConstants.ProductEventHandlerTopic, listener.TopicPatterns.First());
        }

        [TestMethod]
        public void CreateHost_ShouldReturnMicroserviceHostWithEventListenersWhenCalledWithUseConventions()
        {
            var result = target.UseConventions().WithContext(busContextMock.Object).CreateHost();

            var listeners = result.EventListeners.ToList();
            Assert.AreEqual(2, result.EventListeners.Count());
            Assert.IsTrue(listeners.Any(l => l.QueueName == TestClassesConstants.ProductEventListenerQueueName && l.TopicPatterns.Contains(TestClassesConstants.ProductEventHandlerTopic)));
            Assert.IsTrue(listeners.Any(l => l.QueueName == TestClassesConstants.OrderEventListenerQueueName && l.TopicPatterns.Contains(TestClassesConstants.OrderEventHandlerTopic)));
        }

        [TestMethod]
        public void CreateHost_ShouldCallRegisterEventListenersOnMicroserviceHost()
        {
            target.AddEventListener<ProductEventListener>().WithContext(busContextMock.Object);
            var result = target.CreateHost();

            Assert.IsTrue(result.EventListenersRegistered);
        }
    }
}