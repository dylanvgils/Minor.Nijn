using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading;
using Minor.Nijn.RabbitMQBus;
using Minor.Nijn.TestBus.CommandBus;
using Minor.Nijn.TestBus.EventBus;
using RabbitMQ.Client;

namespace Minor.Nijn.TestBus.Test
{
    [TestClass]
    public class TestBusContextTest
    {
        private Mock<IEventBus> _eventBusMock;
        private Mock<ICommandBus> _commandBusMock;

        private TestBusContext _target;

        [TestInitialize]
        public void BeforeEach()
        {
            _eventBusMock = new Mock<IEventBus>(MockBehavior.Strict);
            _commandBusMock = new Mock<ICommandBus>(MockBehavior.Strict);;
            
            _target = new TestBusContext(null, _eventBusMock.Object, _commandBusMock.Object, 500);
        }

        [TestMethod]
        public void CreateMessageReceiver_ShouldReturnTestBusMessageReceiver()
        {
            string queueName = "TestQueue";
            IEnumerable<string> topicExpressions = new List<string> { "a.b.c" };

            var result = _target.CreateMessageReceiver(queueName, topicExpressions);
            
            Assert.IsInstanceOfType(result, typeof(IMessageReceiver));
            Assert.AreEqual(result.QueueName, queueName);
            Assert.AreEqual(result.TopicExpressions, topicExpressions);
        }

        [TestMethod]
        public void CreateMessageReceiver_ShouldThrowExceptionWhenDisposed()
        {
            _target.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => _target.CreateMessageReceiver("QueueName", new List<string>()));
        }

        [TestMethod]
        public void CreateMessageSender_ShouldReturnMessageSender()
        {
            var result = _target.CreateMessageSender();
            Assert.IsInstanceOfType(result, typeof(IMessageSender));
        }

        [TestMethod]
        public void CreateMessageSender_ShouldThrowExceptionWhenDisposed()
        {
            _target.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => _target.CreateMessageSender());
        }

        [TestMethod]
        public void CreateCommandReceiver_ShouldReturnCommandReceiver()
        {
            var result = _target.CreateCommandReceiver("CommandQueue");
            Assert.IsInstanceOfType(result, typeof(ICommandReceiver));
        }

        [TestMethod]
        public void CreateCommandReceiver_ShouldThrowExceptionWhenDisposed()
        {
            _target.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => _target.CreateCommandReceiver("QueueName"));
        }

        [TestMethod]
        public void CreateCommandSender_ShouldReturnCommandSender()
        {
            var result = _target.CreateCommandSender();
            Assert.IsInstanceOfType(result, typeof(ICommandSender));
        }

        [TestMethod]
        public void CreateCommandSender_ShouldThrowExceptionWhenDisposed()
        {
            _target.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => _target.CreateMessageSender());
        }

        [TestMethod]
        public void IsConnectionIdle_ShouldReturnTrueWhenTimeoutExceeded()
        {
            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(conn => conn.Dispose());

            var target = new TestBusContext(connectionMock.Object, _eventBusMock.Object, _commandBusMock.Object, 200);
            Thread.Sleep(500);

            Assert.IsTrue(target.IsConnectionIdle(), "ConnectionIdle should be true");
        }

        [TestMethod]
        public void IsConnectionIdle_ShouldReturnFalseWhenTimeoutNotExceeded()
        {
            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(conn => conn.Dispose());

            var target = new TestBusContext(connectionMock.Object, _eventBusMock.Object, _commandBusMock.Object, 200);
            target.UpdateLastMessageReceived();

            Assert.IsFalse(target.IsConnectionIdle(), "1: ConnectionIdle should be false");

            target.UpdateLastMessageReceived();
            Assert.IsFalse(target.IsConnectionIdle(), "2: ConnectionIdle should be false");

            target.UpdateLastMessageReceived();
            Assert.IsFalse(target.IsConnectionIdle(), "3: ConnectionIdle should be false");
        }

        [TestMethod]
        public void Dispose_ShouldNotThrowException()
        {
            _target.Dispose();
        }
    }
}
