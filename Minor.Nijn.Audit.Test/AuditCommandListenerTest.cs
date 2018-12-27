using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Audit.DAL;
using Minor.Nijn.Audit.Entities;
using Minor.Nijn.Audit.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Minor.Nijn.Audit.Test
{
    [TestClass]
    public class AuditCommandListenerTest
    {
        private Mock<IAuditMessageDataMapper> _dataMapperMock;
        private Mock<IEventReplayer> _replayerMock;

        private AuditCommandListener _target;

        [TestInitialize]
        public void BeforeEach()
        {
            var loggerFactory = new LoggerFactory();
            _dataMapperMock = new Mock<IAuditMessageDataMapper>(MockBehavior.Strict);
            _replayerMock = new Mock<IEventReplayer>(MockBehavior.Strict);

            _target = new AuditCommandListener(_dataMapperMock.Object, _replayerMock.Object, loggerFactory);
        }

        [TestMethod]
        public async Task HandleReplayEventCommands_ShouldHandleReplayEventsCommandMessages()
        {
            var command = new ReplayEventsCommand(Constants.AuditCommandQueueName)
            {
                ExchangeName = "ExchangeName",
                FromTimestamp = new DateTime(2018, 12, 01).Ticks,
                ToTimestamp = new DateTime(2018, 12, 10).Ticks,
                EventType = "Type",
                RoutingKeyExpression = "a.b.c"
            };

            var auditMessages = new List<AuditMessage>()
            {
                new AuditMessage { CorrelationId = "msg1" },
                new AuditMessage { CorrelationId = "msg2" },
            };

            AuditMessageCriteria criteria = null;
            _dataMapperMock.Setup(d => d.FindAuditMessagesByCriteriaAsync(It.IsAny<AuditMessageCriteria>()))
                .Callback<AuditMessageCriteria>(c => criteria = c)
                .ReturnsAsync(auditMessages);

            _replayerMock.Setup(r => r.DeclareExchange(It.IsAny<string>()));
            _replayerMock.Setup(r => r.ReplayAuditMessage(It.IsAny<AuditMessage>()));

            var result = await _target.HandleReplayEventCommands(command);

            _dataMapperMock.VerifyAll();
            _replayerMock.Verify(r => r.DeclareExchange(command.ExchangeName));
            _replayerMock.Verify(r => r.ReplayAuditMessage(It.IsAny<AuditMessage>()), Times.Exactly(2));

            Assert.IsFalse(result.StartTimestamp == 0, "Timestamp should not be 0");
            Assert.AreEqual(command.ExchangeName, result.ExchangeName);
            Assert.AreEqual(auditMessages.Count, result.NumberOfEvents);

            Assert.IsNotNull(criteria, "Criteria should not be null");
            Assert.AreEqual(command.FromTimestamp, criteria.FromTimestamp);
            Assert.AreEqual(command.ToTimestamp, criteria.ToTimestamp);
            Assert.AreEqual(command.EventType, criteria.EventType);
            Assert.AreEqual(command.RoutingKeyExpression, criteria.RoutingKeyExpression);
        }
    }
}