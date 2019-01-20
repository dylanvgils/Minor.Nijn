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
            var queueName = "CommandQueue";
            var message = new RequestCommandMessage("Test message", "type", "id", queueName);
            var command = new TestBusCommand(null, message);

            var mock = new MessageAddedMock<TestBusCommand>();
            var queue = target.DeclareCommandQueue(queueName);
            queue.Subscribe(mock.HandleMessageAdded);

            target.DispatchMessage(command);

            Assert.IsTrue(mock.HandledMessageAddedHasBeenCalled);
            Assert.AreEqual(1, queue.CalledTimes);
            Assert.AreEqual(message, mock.Args.Message.Command);
        }

        [TestMethod]
        public void DispatchMessage_ShouldNotQueueMessageWhenReplyToNotMatches()
        {
            var queueName = "CommandQueue";
            var message = new RequestCommandMessage("Test message", "type", "id", queueName);
            var command = new TestBusCommand(null, message);

            var mock1 = new MessageAddedMock<TestBusCommand>();
            var queue1 = target.DeclareCommandQueue(queueName);
            queue1.Subscribe(mock1.HandleMessageAdded);

            var mock2 = new MessageAddedMock<TestBusCommand>();
            var queue2 = target.DeclareCommandQueue("SomeOtherQueue");
            queue2.Subscribe(mock2.HandleMessageAdded);

            target.DispatchMessage(command);

            Assert.IsTrue(mock1.HandledMessageAddedHasBeenCalled);
            Assert.AreEqual(message, mock1.Args.Message.Command);

            Assert.IsFalse(mock2.HandledMessageAddedHasBeenCalled);
        }

        [TestMethod]
        public void DeclareCommandQueue_ShouldReturnNewCommandBusQueue()
        {
            string name = "RpcQueue";

            var result = target.DeclareCommandQueue("RpcQueue");

            Assert.IsInstanceOfType(result, typeof(CommandBusQueue));
            Assert.AreEqual(name, result.QueueName);
            Assert.AreEqual(1, target.QueueCount);
        }

        [TestMethod]
        public void DeclareQueue_WhenCalledTwiceQueueLengthShouldBe_2()
        {
            target.DeclareCommandQueue("TestQueue1");
            target.DeclareCommandQueue("TestQueue2");
            Assert.AreEqual(target.QueueCount, 2);
        }

        [TestMethod]
        public void DeclareQueue_WhenCalledWithSameQueueNameTwiceLengthShouldBe_1()
        {
            target.DeclareCommandQueue("TestQueue1");
            target.DeclareCommandQueue("TestQueue1");
            Assert.AreEqual(target.QueueCount, 1);
        }
    }
}