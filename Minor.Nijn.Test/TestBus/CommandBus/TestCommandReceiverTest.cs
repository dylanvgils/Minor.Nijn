using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

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
            message.RoutingKey = queueName;
            
            var queue = new CommandBusQueue(queueName);
            contextMock.Setup(context => context.CommandBus.DeclareCommandQueue(It.IsAny<string>()))
                .Returns(queue);
            
            var callbackMock = new Mock<CommandReceivedCallback>(MockBehavior.Strict);
            callbackMock.Setup(callback => callback(It.IsAny<CommandMessage>()))
                .Returns(new CommandMessage("Reply message", "type", "correlationId"));

            target.DeclareCommandQueue();
            target.StartReceivingCommands(callbackMock.Object);
            queue.Enqueue(message);

            callbackMock.Verify(callback => callback(message));
        }

        [TestMethod]
        public void StartReceivingMessages_ShouldThrowBusConfiguratioinExceptionWhenQueueIsNotDeclared()
        {
            CommandMessage message = new CommandMessage("TestMessage", "type", "id");
            var callbackMock = new Mock<CommandReceivedCallback>(MockBehavior.Strict);

            Action action = () =>
            {
                target.StartReceivingCommands(callbackMock.Object);
            };

            var ex = Assert.ThrowsException<BusConfigurationException>(action);
            Assert.AreEqual($"Queue with name: {queueName} is not declared", ex.Message);
        }

        [TestMethod]
        public void Dispose_ShouldNotThrowException()
        {
            target.Dispose();
        }
    }
}