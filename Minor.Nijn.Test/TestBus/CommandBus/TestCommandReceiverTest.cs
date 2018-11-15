using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Minor.Nijn.TestBus.CommandBus;
using Moq;

namespace Minor.Nijn.TestBus.CommandBus.Test
{
    [TestClass]
    public class TestCommandReceiverTest
    {
        private string queueName;
        private Mock<ITestBusContext> contextMock;
        private TestCommandReceiver target;
        
        [TestInitialize]
        public void BeforeEach()
        {
            queueName = "RpcQueue";
            
            contextMock = new Mock<ITestBusContext>(MockBehavior.Strict);
            target = new TestCommandReceiver(contextMock.Object, queueName);
        }
        
        [TestMethod]
        public void DeclareCommandQueue_ShouldDeclareAQueueOnTheEventBuzz()
        {
            contextMock.Setup(context => context.CommandBus.DeclareCommandQueue(It.IsAny<string>()))
                .Returns(new CommandBusQueue(queueName));
            
            target.DeclareCommandQueue();
            
            contextMock.Verify(context => context.CommandBus.DeclareCommandQueue(queueName));
        }
        
        [TestMethod]
        public void StartReceivingMessages_ShouldStartListeningForMessagesOnRpcQueue()
        {
            CommandMessage message = new CommandMessage("TestMessage", "type" ,"id");
            message.ReplyTo = queueName;
            
            var queue = new CommandBusQueue(queueName);
            contextMock.Setup(context => context.CommandBus.DeclareCommandQueue(It.IsAny<string>()))
                .Returns(queue);
            
            var callbackMock = new Mock<CommandReceivedCallback>(MockBehavior.Strict);
            callbackMock.Setup(callback => callback(It.IsAny<CommandMessage>()));

            target.DeclareCommandQueue();
            target.StartReceivingCommands(callbackMock.Object);
            queue.Enqueue(message);

            callbackMock.Verify(callback => callback(message));
        }
        
        [TestMethod]
        public void Dispose_ShouldNotThrowException()
        {
            target.Dispose();
        }
    }
}