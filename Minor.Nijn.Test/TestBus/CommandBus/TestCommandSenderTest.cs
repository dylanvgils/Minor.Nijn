using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var response = new CommandMessage("Reply message", "type", "id");

            CommandBusQueue commandQueue = null;
            contextMock.Setup(ctx => ctx.CommandBus.DeclareCommandQueue(It.IsAny<string>()))
                .Callback<string>(name =>
                {
                    response.RoutingKey = name;
                    commandQueue = new CommandBusQueue(name);
                })
                .Returns(() => commandQueue);

            contextMock.Setup(ctx => ctx.CommandBus.DispatchMessage(request));

            var result = target.SendCommandAsync(request);
            commandQueue.Enqueue(response);

            contextMock.VerifyAll();
            Assert.AreEqual(response, result.Result);
        }

        [TestMethod]
        public void SendCommandAsync_ShouldThrowTimeoutExceptionAfter_5_Seconds()
        {
            var request = new CommandMessage("Test message.", "type", "id");
            var response = new CommandMessage("Reply message", "type", "id");

            CommandBusQueue commandQueue = null;
            contextMock.Setup(ctx => ctx.CommandBus.DeclareCommandQueue(It.IsAny<string>()))
                .Callback<string>(name =>
                {
                    response.RoutingKey = name;
                    commandQueue = new CommandBusQueue(name);
                })
                .Returns(() => commandQueue);

            contextMock.Setup(ctx => ctx.CommandBus.DispatchMessage(request));

            var result = target.SendCommandAsync(request);

            contextMock.VerifyAll();

            while(!result.IsFaulted) { }
            Assert.IsTrue(result.IsFaulted);
        }

        [TestMethod]
        public void Dispose_ShouldNotThrowException()
        {
            target.Dispose();
        }
    }
}