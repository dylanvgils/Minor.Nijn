using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQbusContextTest
    {
        private string _exchangeName = "ExchangeName";

        private Mock<IConnection> _connectionMock;
        private Mock<IModel> _channelMock;

        private RabbitMQBusContext _target;

        [TestInitialize]
        public void BeforeEach()
        {
            _connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            _channelMock = new Mock<IModel>(MockBehavior.Strict);

            _connectionMock.Setup(conn => conn.CreateModel()).Returns(_channelMock.Object);

            _target = new RabbitMQBusContext(_connectionMock.Object, _exchangeName, Constants.RabbitMQConnectionTimeoutAfterMs, false);
        }
        
        [TestMethod]
        public void CreateMessageSender_ShouldReturnIMessageSender()
        {            
            var result = _target.CreateMessageSender();
            
            _connectionMock.VerifyAll();
            Assert.IsInstanceOfType(result, typeof(IMessageSender));
        }

        [TestMethod]
        public void CreateMessageSender_ShouldThrowExceptionWhenDisposed()
        {
            _connectionMock.Setup(conn => conn.Dispose());
            _target.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => _target.CreateMessageSender());
        }

        [TestMethod]
        public void CreateMessageReceiver_ShouldReturnIMessageReceiver()
        {
            var queueName = "queueName";
            IEnumerable<string> topicExpressions = new List<string> { "a.b.c" };
            
            var result = _target.CreateMessageReceiver(queueName, topicExpressions);
            
            _connectionMock.VerifyAll();
            Assert.IsInstanceOfType(result, typeof(IMessageReceiver));
        }

        [TestMethod]
        public void CreateMessageReceiver_ShouldThrowExceptionWhenDisposed()
        {
            _connectionMock.Setup(conn => conn.Dispose());
            _target.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => _target.CreateMessageReceiver("Queue", new List<string>()));
        }
        
        [TestMethod]
        public void CreateCommandSender_ShouldReturnICommandSender()
        {
            var result = _target.CreateCommandSender();

            _connectionMock.VerifyAll();
            Assert.IsInstanceOfType(result, typeof(ICommandSender));
        }

        [TestMethod]
        public void CreateCommandSender_ShouldThrowExceptionWhenDisposed()
        {
            _connectionMock.Setup(conn => conn.Dispose());
            _target.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => _target.CreateCommandSender());
        }

        [TestMethod]
        public void CreateCommandReceiver_ShouldReturnICommandReceiver()
        {
            var result = _target.CreateCommandReceiver("queueName");

            _connectionMock.VerifyAll();
            Assert.IsInstanceOfType(result, typeof(ICommandReceiver));
        }

        [TestMethod]
        public void CreateCommandReceiver_ShouldThrowExceptionWhenDisposed()
        {
            _connectionMock.Setup(conn => conn.Dispose());
            _target.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => _target.CreateCommandReceiver("QueueName"));
        }

        [TestMethod]
        public void IsConnectionIdle_ShouldReturnTrueWhenTimeoutExceeded()
        {
            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(conn => conn.Dispose());

            var target = new RabbitMQBusContext(connectionMock.Object, _exchangeName, 200, true);
            Thread.Sleep(500);

            Assert.IsTrue(target.IsConnectionIdle(), "ConnectionIdle should be true");
        }

        [TestMethod]
        public void IsConnectionIdle_ShouldReturnFalseWhenTimeoutNotExceeded()
        {
            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(conn => conn.Dispose());

            var target = new RabbitMQBusContext(connectionMock.Object, _exchangeName, 200, true);
            target.UpdateLastMessageReceived();

            Assert.IsFalse(target.IsConnectionIdle(), "1: ConnectionIdle should be false");

            target.UpdateLastMessageReceived();
            Assert.IsFalse(target.IsConnectionIdle(), "2: ConnectionIdle should be false");

            target.UpdateLastMessageReceived();
            Assert.IsFalse(target.IsConnectionIdle(), "3: ConnectionIdle should be false");
        }

        [TestMethod]
        public void Dispose_ShouldCallDisposeOnResources()
        {
            _connectionMock.Setup(conn => conn.Dispose());

            _target.Dispose();
            _target.Dispose(); // Don't call dispose for second time

            _connectionMock.Verify(conn => conn.Dispose(), Times.Once);
        }

        [TestMethod]
        public void RabbitMQBusContext_ShouldCallDisposeWhenIdleTimeExceededAndAutoDisconnectEnabled()
        {
            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(conn => conn.Dispose());

            new RabbitMQBusContext(connectionMock.Object, _exchangeName, 200, true);
            Thread.Sleep(500);

            connectionMock.Verify(conn => conn.Dispose(), Times.Once);
        }
    }
}
