using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Minor.Nijn.TestBus.CommandBus.Test
{
    [TestClass]
    public class CommandQueueTest
    {
        private string _queueName;
        private CommandBusQueue _target;

        [TestInitialize]
        public void BeforeEach()
        {
            _queueName = "queueName";
            _target = new CommandBusQueue(_queueName);
        }

        [TestMethod]
        public void Enqueue_ShouldEnqueueMessageWithCorrectRoutingKey()
        {
            var command = new TestBusCommand("replyTo", new RequestCommandMessage("message1", "type1", "corId", _queueName));
            _target.Enqueue(command);
            Assert.AreEqual(1, _target.MessageQueueLength);
        }

        [TestMethod]
        public void Enqueue_ShouldNotEnqueueMessagesWithWrongRoutingKey()
        {
            var command = new TestBusCommand("replyTo", new RequestCommandMessage("message1", "type1", "corId", "wrongName"));
            _target.Enqueue(command);
            Assert.AreEqual(0, _target.MessageQueueLength);
        }

        [TestMethod]
        public void Indexer_ShouldReturnItemAtIndex()
        {
            var msg1 = new TestBusCommand("replyTo", new RequestCommandMessage("message1", "type1", "corId", _queueName));
            var msg2 = new TestBusCommand("replyTo", new RequestCommandMessage("message2", "type2", "corId", _queueName));
            var msg3 = new TestBusCommand("replyTo", new RequestCommandMessage("message3", "type3", "corId", _queueName));

            _target.Enqueue(msg1);
            _target.Enqueue(msg2);
            _target.Enqueue(msg3);

            Assert.AreEqual(msg1, _target[0]);
            Assert.AreEqual(msg2, _target[1]);
            Assert.AreEqual(msg3, _target[2]);
        }
    }
}