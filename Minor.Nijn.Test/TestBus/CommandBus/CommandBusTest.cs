using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus.Mocks.Test;

namespace Minor.Nijn.TestBus.CommandBus.Test
{
    [TestClass]
    public class CommandBusTest
    {
        private CommandBus target;
        
        [TestInitialize]
        public void BeforeEach()
        {
            target = new CommandBus();
        }
        
        [TestMethod]
        public void DispatchMessage_ShouldTriggerEvent()
        {
            var message = new CommandMessage("Test message", "type", "id");
            message.ReplyTo = "ReplyQueue1";
            
            var mock = new MessageAddedMock<CommandMessage>();
            var queue = target.DeclareCommandQueue("ReplyQueue1");
            queue.MessageAdded += mock.HandleMessageAdded;

            target.DispatchMessage(message);

            Assert.IsTrue(mock.HandledMessageAddedHasBeenCalled);
            Assert.AreEqual(message, mock.Args.Message);
        }

        [TestMethod]
        public void DispatchMessage_ShouldNotQueueMessageWhenReplyToNotMatches()
        {
            var message = new CommandMessage("Test message", "type", "id");
            message.ReplyTo = "ReplyQueue1";

            var mock1 = new MessageAddedMock<CommandMessage>();
            var queue1 = target.DeclareCommandQueue("ReplyQueue1");
            queue1.MessageAdded += mock1.HandleMessageAdded;

            var mock2 = new MessageAddedMock<CommandMessage>();
            var queue2 = target.DeclareCommandQueue("ReplyQueue2");
            queue2.MessageAdded += mock2.HandleMessageAdded;

            target.DispatchMessage(message);

            Assert.IsTrue(mock1.HandledMessageAddedHasBeenCalled);
            Assert.AreEqual(message, mock1.Args.Message);

            Assert.IsFalse(mock2.HandledMessageAddedHasBeenCalled);
        }
        
        [TestMethod]
        public void DeclareCommandQueue_ShouldReturnNewCommandBusQueue()
        {
            string name = "RpcQueue";
           
            var result = target.DeclareCommandQueue("RpcQueue");
            
            Assert.IsInstanceOfType(result, typeof(CommandBusQueue));
            Assert.AreEqual(name, result.QueueName);
            Assert.AreEqual(1, target.QueueLength);
        }
        
        [TestMethod]
        public void DeclareQueue_WhenCalledTwiceQueueLengthShouldBe_2()
        {
            target.DeclareCommandQueue("TestQueue1");
            target.DeclareCommandQueue("TestQueue2");
            Assert.AreEqual(target.QueueLength, 2);
        }

        [TestMethod]
        public void DeclareQueue_WhenCalledWithSameQueueNameTwiceLengthShouldBe_1()
        {
            target.DeclareCommandQueue("TestQueue1");
            target.DeclareCommandQueue("TestQueue1");
            Assert.AreEqual(target.QueueLength, 1);
        }
    }
}