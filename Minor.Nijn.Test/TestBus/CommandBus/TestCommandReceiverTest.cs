using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;

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
        public void DeclareCommandQueue_ShouldDeclareAQueueOnTheEventBuss()
        {
            contextMock.Setup(context => context.CommandBus.DeclareCommandQueue(It.IsAny<string>()))
                .Returns(new CommandBusQueue(queueName));
            
            target.DeclareCommandQueue();
            
            contextMock.Verify(context => context.CommandBus.DeclareCommandQueue(queueName));
        }

        [TestMethod]
        public void DeclareCommandQueue_ShouldThrowExceptionWhenAlreadyDeclared()
        {
            contextMock.Setup(context => context.CommandBus.DeclareCommandQueue(queueName))
                .Returns(new CommandBusQueue(queueName));

            target.DeclareCommandQueue();
            Action action = () => { target.DeclareCommandQueue(); };

            contextMock.VerifyAll();

            var ex = Assert.ThrowsException<BusConfigurationException>(action);
            Assert.AreEqual($"Queue with name: {queueName} is already declared", ex.Message);
        }

        [TestMethod]
        public void DeclareCommandQueue_ShouldThrowExceptionWhenDisposed()
        {
            target.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => target.DeclareCommandQueue());
        }

        [TestMethod]
        public void StartReceivingCommands_ShouldStartListeningForMessagesOnRpcQueue()
        {
            var message = new RequestCommandMessage("TestMessage", "type", "id", queueName);
            var command = new TestBusCommand(null, message);

            var queue = new CommandBusQueue(queueName);
            contextMock.Setup(context => context.CommandBus.DeclareCommandQueue(It.IsAny<string>()))
                .Returns(queue);
            
            var callbackMock = new Mock<CommandReceivedCallback>(MockBehavior.Strict);
            callbackMock.Setup(callback => callback(It.IsAny<RequestCommandMessage>()))
                .Returns(new ResponseCommandMessage("Reply message", "type", "correlationId"));

            target.DeclareCommandQueue();
            target.StartReceivingCommands(callbackMock.Object);
            queue.Enqueue(command);

            callbackMock.Verify(callback => callback(message));
        }

        [TestMethod]
        public void StartReceivingCommands_ShouldThrowBusConfigurationExceptionWhenQueueIsNotDeclared()
        {
            var callbackMock = new Mock<CommandReceivedCallback>(MockBehavior.Strict);

            Action action = () =>
            {
                target.StartReceivingCommands(callbackMock.Object);
            };

            var ex = Assert.ThrowsException<BusConfigurationException>(action);
            Assert.AreEqual($"Queue with name: {queueName} is not declared", ex.Message);
        }

        [TestMethod]
        public void StartReceivingCommands_ShouldUseReplyToWhenSet()
        {
            var replyQueueName = "ReplyCommandToQueue";
            var request = new RequestCommandMessage("TestMessage", "type", "id", queueName);
            var command = new TestBusCommand(replyQueueName, request);

            var response = new ResponseCommandMessage("Reply message", "type", "correlationId");

            var commandQueue = new Dictionary<string, CommandBusQueue>();
            commandQueue.Add(replyQueueName, new CommandBusQueue(replyQueueName));

            var queue = new CommandBusQueue(queueName);
            contextMock.Setup(ctx => ctx.CommandBus.DeclareCommandQueue(It.IsAny<string>()))
                .Returns(queue);
            contextMock.SetupGet(ctx => ctx.CommandBus.Queues).Returns(commandQueue);

            var callbackMock = new Mock<CommandReceivedCallback>(MockBehavior.Strict);
            callbackMock.Setup(callback => callback(It.IsAny<RequestCommandMessage>()))
                .Returns(response);

            target.DeclareCommandQueue();
            target.StartReceivingCommands(callbackMock.Object);
            queue.Enqueue(command);

            callbackMock.Verify(callback => callback(request));
            Assert.AreEqual(1, commandQueue[replyQueueName].MessageQueueLength);
        }

        [TestMethod]
        public void StartReceivingCommands_ShouldThrowExceptionWhenDisposed()
        {
            var response = new ResponseCommandMessage("message", "type", "id");
            target.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => target.StartReceivingCommands((c) => response));
        }

        [TestMethod]
        public void Dispose_ShouldNotThrowException()
        {
            target.Dispose();
        }
    }
}