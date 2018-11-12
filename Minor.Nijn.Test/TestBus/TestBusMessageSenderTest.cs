using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Moq;

namespace Minor.Nijn.Test.TestBus
{
    [TestClass]
    public class TestBusMessageSenderTest
    {
        private Mock<IBusContextExtension> contextMock;
        private TestBusMessageSender target;

        [TestInitialize]
        public void BeforeEach()
        {
            contextMock = new Mock<IBusContextExtension>(MockBehavior.Strict);
            target = new TestBusMessageSender(contextMock.Object);
        }
        
        [TestMethod]
        public void SendMessage_ShouldCallDispatchMessage()
        {
            var message = new EventMessage("a.b.c", "Test message.");
            contextMock.Setup(context => context.TestBuzz.DispatchMessage(It.IsAny<EventMessage>()));

            target.SendMessage(message);

            contextMock.Verify(context => context.TestBuzz.DispatchMessage(message));
        }
    }
}
