using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Minor.Nijn.TestBus.EventBus;
using Moq;

namespace Minor.Nijn.TestBus.EventBus.Test
{
    [TestClass]
    public class TestMessageSenderTest
    {
        private Mock<ITestBusContext> contextMock;
        private TestMessageSender target;

        [TestInitialize]
        public void BeforeEach()
        {
            contextMock = new Mock<ITestBusContext>(MockBehavior.Strict);
            target = new TestMessageSender(contextMock.Object);
        }
        
        [TestMethod]
        public void SendMessage_ShouldCallDispatchMessage()
        {
            var message = new EventMessage("a.b.c", "Test message.");
            contextMock.Setup(context => context.EventBus.DispatchMessage(It.IsAny<EventMessage>()));

            target.SendMessage(message);

            contextMock.Verify(context => context.EventBus.DispatchMessage(message));
        }

        [TestMethod]
        public void SendMessage_ShouldThrowExceptionWhenDisposed()
        {
            var message = new EventMessage("RoutingKey", "message", "type");
            target.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => target.SendMessage(message));
        }

        [TestMethod]
        public void Dispose_ShouldNotThrowException()
        {
            target.Dispose();
        }
    }
}
