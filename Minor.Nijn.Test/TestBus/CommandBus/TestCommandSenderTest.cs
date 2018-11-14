using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Minor.Nijn.TestBus.CommandBus;
using Moq;

namespace Minor.Nijn.TestBus.CommandBus.Test
{
    [TestClass]
    public class TestCommandSenderTest
    {
        private Mock<IBusContextExtension> contextMock;
        private TestCommandSender target;

        [TestInitialize]
        public void BeforeEach()
        {
            contextMock = new Mock<IBusContextExtension>(MockBehavior.Strict);
            target = new TestCommandSender(contextMock.Object);
        }
        
        [TestMethod]
        public void SendCommandAsync_ShouldCallDispatchMessage()
        {
            var message = new CommandMessage("Test message.", "type", "id");
            
            var queueMock = new Mock<CommandBusQueue>(MockBehavior.Strict);
            contextMock.Setup(context => context.CommandBus.DeclareCommandQueue(It.IsAny<string>())).Returns(queueMock.Object);
            contextMock.Setup(context => context.CommandBus.DispatchMessage(It.IsAny<CommandMessage>())).Callback(() =>
            {
                queueMock.Raise(queue => queue.MessageAdded += null, this, new MessageAddedEventArgs<CommandMessage>(message));
            });
            
            var result = target.SendCommandAsync(message);

            contextMock.Verify(context => context.CommandBus.DispatchMessage(message));
            contextMock.Verify(context => context.CommandBus.DeclareCommandQueue(result.Result.ReplyTo));
            
            Assert.AreEqual(message, result.Result);
        }
        
        [TestMethod]
        public void SendCommandAsync_ShouldCallDispatchMessageWithReplyToQueueName()
        {
            var message = new CommandMessage("Test message.", "type", "id");
            message.ReplyTo = "ReplyQueue1";
            
            var queueMock = new Mock<CommandBusQueue>(MockBehavior.Strict);
            contextMock.Setup(context => context.CommandBus.DeclareCommandQueue(It.IsAny<string>())).Returns(queueMock.Object);
            contextMock.Setup(context => context.CommandBus.DispatchMessage(It.IsAny<CommandMessage>())).Callback(() =>
            {
                queueMock.Raise(queue => queue.MessageAdded += null, this, new MessageAddedEventArgs<CommandMessage>(message));
            });
            
            var result = target.SendCommandAsync(message);

            contextMock.Verify(context => context.CommandBus.DispatchMessage(message));
            contextMock.Verify(context => context.CommandBus.DeclareCommandQueue("ReplyQueue1"));
            
            Assert.AreEqual(message, result.Result);
        }
        
        [TestMethod]
        public void Dispose_ShouldNotThrowException()
        {
            target.Dispose();
        }
    }
}