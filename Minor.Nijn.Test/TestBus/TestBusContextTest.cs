using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using Minor.Nijn.TestBus.CommandBus;
using Minor.Nijn.TestBus.EventBus;

namespace Minor.Nijn.TestBus.Test
{
    [TestClass]
    public class TestBusContextTest
    {
        private Mock<IEventBus> eventBusMock;
        private Mock<ICommandBus> commandBusMock;
        private TestBusContext target;

        [TestInitialize]
        public void BeforeEach()
        {
            eventBusMock = new Mock<IEventBus>(MockBehavior.Strict);
            commandBusMock = new Mock<ICommandBus>(MockBehavior.Strict);;
            
            target = new TestBusContext(eventBusMock.Object, commandBusMock.Object, "CommandQueue");
        }

        [TestMethod]
        public void CreateMessageReceiver_ShouldReturnTestBusMessageReceiver()
        {
            string queueName = "TestQueue";
            IEnumerable<string> topicExpressions = new List<string> { "a.b.c" };
            
            eventBusMock.Setup(eventBus => eventBus.DeclareQueue(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(new EventBusQueue(queueName, topicExpressions));    

            var result = target.CreateMessageReceiver(queueName, topicExpressions);

            eventBusMock.Verify(eventBus => eventBus.DeclareQueue(queueName, topicExpressions));
            
            Assert.IsInstanceOfType(result, typeof(IMessageReceiver));
            Assert.AreEqual(result.QueueName, queueName);
            Assert.AreEqual(result.TopicExpressions, topicExpressions);
        }

        [TestMethod]
        public void CreateMessageSender_ShouldReturnMessageSender()
        {
            var result = target.CreateMessageSender();
            Assert.IsInstanceOfType(result, typeof(IMessageSender));
        }
        
        [TestMethod]
        public void CreateCommandReceiver_ShouldReturnCommandReceiver()
        {
            var result = target.CreateCommandReceiver("CommandQueue");
            Assert.IsInstanceOfType(result, typeof(ICommandReceiver));
        }
        
        [TestMethod]
        public void CreateCommandSender_ShouldReturnCommandSender()
        {
            var result = target.CreateCommandSender();
            Assert.IsInstanceOfType(result, typeof(ICommandSender));
        }

        [TestMethod]
        public void CreateMockCommandSender_ShouldReturnTestCommandSender()
        {
            var result = target.CreateCommandSender();
            Assert.IsInstanceOfType(result, typeof(IMockCommandSender));
        }

        [TestMethod]
        public void SendMockCommand_ShouldCallDispatchMessage()
        {
            commandBusMock.Setup(commandBus => commandBus.DispatchMessage(It.IsAny<CommandMessage>()));
            var command = new CommandMessage("Test message", "type", "id");

            target.SendMockCommand(command);

            commandBusMock.Verify(commandBus => commandBus.DispatchMessage(command));
        }
        
        [TestMethod]
        public void Dispose_ShouldNotThrowException()
        {
            target.Dispose();
        }
    }
}
