using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Audit.DAL;
using Minor.Nijn.Audit.Entities;
using Moq;
using System;
using System.Threading.Tasks;

namespace Minor.Nijn.Audit.Test
{
    [TestClass]
    public class AuditEventListenerTest
    {
        private Mock<IAuditMessageDataMapper> _dataMapperMock;

        private AuditEventListener _target;

        [TestInitialize]
        public void BeforeEach()
        {
            _dataMapperMock = new Mock<IAuditMessageDataMapper>(MockBehavior.Strict);
            _target = new AuditEventListener(_dataMapperMock.Object);
        }

        [TestMethod]
        public async Task HandleEvents_ShouldHandleEventMessages()
        {
            AuditMessage result = null;
            _dataMapperMock.Setup(d => d.InsertAsync(It.IsAny<AuditMessage>()))
                .Callback((AuditMessage m) => result = m)
                .Returns(Task.CompletedTask);

            var message = new EventMessage(
                routingKey: "RoutingKey",
                message: "Message",
                type: "type",
                timestamp: DateTime.Now.Ticks,
                correlationId: "CorrelationId"
            );

            await _target.HandleEvents(message);

            _dataMapperMock.VerifyAll();

            Assert.IsNotNull(result, "AuditMessage should not be null");
            Assert.AreEqual(message.RoutingKey, result.RoutingKey);
            Assert.AreEqual(message.Message, result.Payload);
            Assert.AreEqual(message.Type, result.Type);
            Assert.AreEqual(message.Timestamp, result.Timestamp);
        }
    }
}