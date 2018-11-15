using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Minor.Nijn.TestBus.CommandBus;
using Moq;

namespace Minor.Nijn.TestBus.CommandBus.Test
{
    [TestClass]
    public class TestCommandSenderTest
    {
        private Mock<ITestBusContext> contextMock;
        private TestCommandSender target;

        [TestInitialize]
        public void BeforeEach()
        {
            contextMock = new Mock<ITestBusContext>(MockBehavior.Strict);
            target = new TestCommandSender(contextMock.Object);
        }
        
        [TestMethod]
        public void SendCommandAsync_ShouldCallDispatchMessage()
        {
            var request = new CommandMessage("Test message.", "type", "id");
            request.ReplyTo = "ReplyQueue1";            
            var response = new CommandMessage("Reply message", "type", "id");
            target.ReplyMessage = response;
            
            var result = target.SendCommandAsync(request);

            Assert.AreEqual(result.Result, response);
        }
        
        [TestMethod]
        public void Dispose_ShouldNotThrowException()
        {
            target.Dispose();
        }
    }
}