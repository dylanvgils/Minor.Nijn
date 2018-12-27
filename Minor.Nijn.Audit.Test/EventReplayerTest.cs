using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Audit.Entities;
using Moq;
using RabbitMQ.Client;
using System;
using System.Text;

namespace Minor.Nijn.Audit.Test
{
    [TestClass]
    public class EventReplayerTest
    {

        private Mock<IModel> _channelMock;

        private EventReplayer _target;

        [TestInitialize]
        public void BeforeEach()
        {
            _channelMock = new Mock<IModel>(MockBehavior.Strict);

            var connectionMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            connectionMock.Setup(conn => conn.Connection.CreateModel()).Returns(_channelMock.Object);

            var loggerFactory = new LoggerFactory();

            _target = new EventReplayer(connectionMock.Object, loggerFactory);
        }

        [TestMethod]
        public void DeclareExchange_ShouldDeclareExchange()
        {
            var exchangeName = "exchangeName";
            _channelMock.Setup(chan => chan.ExchangeDeclare(exchangeName, Constants.ReplayerExchangeType, false, true, null));

            _target.DeclareExchange(exchangeName);

            _channelMock.VerifyAll();
        }

        [TestMethod]
        public void DeclareExchange_ShouldThrowInvalidOperationExceptionWhenExchangeAlreadyDeclared()
        {
            var exchangeName = "exchangeName";
            _channelMock.Setup(chan => chan.ExchangeDeclare(exchangeName, Constants.ReplayerExchangeType, false, true, null));

            _target.DeclareExchange(exchangeName);
            Action action = () => { _target.DeclareExchange(exchangeName); };

            _channelMock.VerifyAll();

            var ex = Assert.ThrowsException<InvalidOperationException>(action);
            Assert.AreEqual($"Exchange with name: {exchangeName} already declared", ex.Message);
        }

        [TestMethod]
        public void ReplayAuditMessage_ShouldPublishAuditMessage()
        {
            var exchangeName = "exchangeName";

            var message = new AuditMessage
            {
                Id = 1,
                RoutingKey = "RoutingKey",
                CorrelationId = "CorrelationId",
                Type = "Type",
                Timestamp = DateTime.Now.Ticks,
                Payload = "Payload"
            };

            var propsMock = new Mock<IBasicProperties>(MockBehavior.Strict);
            propsMock.SetupSet(props => props.CorrelationId = message.CorrelationId);
            propsMock.SetupSet(props => props.Timestamp = new AmqpTimestamp(message.Timestamp));
            propsMock.SetupSet(props => props.Type = message.Type);

            _channelMock.Setup(chan => chan.ExchangeDeclare(exchangeName, Constants.ReplayerExchangeType, false, true, null));
            _channelMock.Setup(chan => chan.CreateBasicProperties()).Returns(propsMock.Object);
            _channelMock.Setup(chan => chan.BasicPublish(
                exchangeName,
                message.RoutingKey,
                false,
                propsMock.Object,
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == message.Payload)
            ));

            _target.DeclareExchange(exchangeName);
            _target.ReplayAuditMessage(message);

            propsMock.VerifyAll();
            _channelMock.VerifyAll();
        }

        [TestMethod]
        public void ReplayAuditMessage_ShouldSetCorrelationIdWhenNull()
        {
            var exchangeName = "exchangeName";

            var message = new AuditMessage
            {
                Id = 1,
                RoutingKey = "RoutingKey",
                Type = "Type",
                Timestamp = DateTime.Now.Ticks,
                Payload = "Payload"
            };

            var propsMock = new Mock<IBasicProperties>(MockBehavior.Strict);
            propsMock.SetupSet(props => props.CorrelationId = It.IsAny<string>());
            propsMock.SetupSet(props => props.Timestamp = new AmqpTimestamp(message.Timestamp));
            propsMock.SetupSet(props => props.Type = message.Type);

            _channelMock.Setup(chan => chan.ExchangeDeclare(exchangeName, Constants.ReplayerExchangeType, false, true, null));
            _channelMock.Setup(chan => chan.CreateBasicProperties()).Returns(propsMock.Object);
            _channelMock.Setup(chan => chan.BasicPublish(
                exchangeName,
                message.RoutingKey,
                false,
                propsMock.Object,
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == message.Payload)
            ));

            _target.DeclareExchange(exchangeName);
            _target.ReplayAuditMessage(message);

            propsMock.VerifyAll();
            _channelMock.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void ReplayAuditMessage_ShouldThrowExceptionWhenDisposed()
        {
            _channelMock.Setup(chan => chan.Dispose());

            var message = new AuditMessage();

            _target.Dispose();
            _target.ReplayAuditMessage(message);
        }

        [TestMethod]
        public void ReplayAuditMessage_ShouldThrowExceptionWhenExchangeIsNotDeclared()
        {
            var message = new AuditMessage();

            Action action = () => { _target.ReplayAuditMessage(message); };

            var ex = Assert.ThrowsException<InvalidOperationException>(action);
            Assert.AreEqual("Exchange should be declared", ex.Message);
        }

        [TestMethod]
        public void CreateCommandReceiver_ShouldThrowExceptionWhenDisposed()
        {
            _channelMock.Setup(conn => conn.Dispose());
            _target.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => _target.ReplayAuditMessage(new AuditMessage()));
        }

        [TestMethod]
        public void Dispose_ShouldCallDisposeOnResources()
        {
            _channelMock.Setup(conn => conn.Dispose());

            _target.Dispose();
            _target.Dispose(); // Don't call dispose for second time

            _channelMock.Verify(conn => conn.Dispose(), Times.Once);
        }
    }
}