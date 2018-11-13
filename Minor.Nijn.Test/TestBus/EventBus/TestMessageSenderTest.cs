using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Minor.Nijn.TestBus.EventBus;
using Moq;

namespace Minor.Nijn.Test.TestBus
{
    [TestClass]
    public class TestMessageSenderTest
    {
        private Mock<IBusContextExtension> contextMock;
        private TestMessageSender target;

        [TestInitialize]
        public void BeforeEach()
        {
            contextMock = new Mock<IBusContextExtension>(MockBehavior.Strict);
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
    }
}
