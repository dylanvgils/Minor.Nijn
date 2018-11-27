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
            var request = new RequestCommandMessage("Test message.", "type", "id");

            var response = new ResponseCommandMessage("Reply message", "type", "id");
            var responseCommand = new TestBusCommand(null, response);

            CommandBusQueue commandQueue = null;
            contextMock.Setup(ctx => ctx.CommandBus.DeclareCommandQueue(It.IsAny<string>()))
                .Callback<string>(name =>
                {
                    response.RoutingKey = name;
                    commandQueue = new CommandBusQueue(name);
                })
                .Returns(() => commandQueue);

            contextMock.Setup(ctx => ctx.CommandBus.DispatchMessage(It.IsAny<TestBusCommand>()));

            var result = target.SendCommandAsync(request);
            commandQueue.Enqueue(responseCommand);

            contextMock.VerifyAll();
            Assert.AreEqual(response, result.Result);
        }

        [TestMethod]
        public void SendCommandAsync_ShouldThrowTimeoutExceptionAfter_5_Seconds()
        {
            var correlationId = "correlationId";
            var request = new RequestCommandMessage("Test message.", "type", correlationId);
            var response = new ResponseCommandMessage("Reply message", "type", correlationId);

            CommandBusQueue commandQueue = null;
            contextMock.Setup(ctx => ctx.CommandBus.DeclareCommandQueue(It.IsAny<string>()))
                .Callback<string>(name =>
                {
                    response.RoutingKey = name;
                    commandQueue = new CommandBusQueue(name);
                })
                .Returns(() => commandQueue);

            contextMock.Setup(ctx => ctx.CommandBus.DispatchMessage(It.IsAny<TestBusCommand>()))
                .Callback((TestBusCommand c) => Assert.AreEqual(correlationId, c.CorrelationId));

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