using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Audit.DAL;
using Minor.Nijn.Audit.Entities;
using Minor.Nijn.Audit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minor.Nijn.Audit.Test.DAL
{
    [TestClass]
    public class EventMessageDataMapperTest
    {
        private SqliteConnection _connection;
        private DbContextOptions<AuditContext> _options;

        private IAuditMessageDataMapper _target;

        [TestInitialize]
        public void BeforeEach()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _options = new DbContextOptionsBuilder<AuditContext>()
                .UseSqlite(_connection)
                .Options;

            using (var context = new AuditContext(_options))
            {
                context.Database.EnsureCreated();
                SeedDatabase(context);
            }

            _target = new AuditMessageDataMapper(_options);
        }

        private static void SeedDatabase(AuditContext context)
        {
            var messages = new List<AuditMessage>
            {
                new AuditMessage { RoutingKey = "a.y.z", CorrelationId = "msg1", Type = "Type2", Timestamp = new DateTime(2018, 11, 20).Ticks, Payload = "Payload" },
                new AuditMessage { RoutingKey = "a.b.x", CorrelationId = "msg2", Type = "Type1", Timestamp = new DateTime(2018, 12, 08).Ticks, Payload = "Payload" },
                new AuditMessage { RoutingKey = "x.y.z", CorrelationId = "msg3", Type = "Type1", Timestamp = new DateTime(2018, 12, 09).Ticks, Payload = "Payload" },
                new AuditMessage { RoutingKey = "a.b.c", CorrelationId = "msg4", Type = "Type1", Timestamp = new DateTime(2018, 12, 10).Ticks, Payload = "Payload" },
                new AuditMessage { RoutingKey = "a.b.c", CorrelationId = "msg5", Type = "Type2", Timestamp = new DateTime(2018, 12, 11).Ticks, Payload = "Payload" },
            };

            context.AuditMessages.AddRange(messages);
            context.SaveChanges();
        }

        [TestCleanup]
        public void AfterEach()
        {
            _connection.Dispose();
        }

        [TestMethod]
        public async Task InsertAsync_ShouldInsertEventMessage()
        {
            var message = new AuditMessage
            {
                RoutingKey = "RoutingKey",
                CorrelationId = "CorrelationId",
                Type = "Type",
                Timestamp = DateTime.Now.Ticks,
                Payload = "Payload"
            };

            await _target.InsertAsync(message);

            using (var context = new AuditContext(_options))
            {
                var result = context.AuditMessages.SingleOrDefault(m => m.Id == 6);

                Assert.IsNotNull(result, "Result should not be null");
                Assert.AreEqual(message.RoutingKey, result.RoutingKey);
                Assert.AreEqual(message.CorrelationId, result.CorrelationId);
                Assert.AreEqual(message.Type, result.Type);
                Assert.AreEqual(message.Timestamp, result.Timestamp);
                Assert.AreEqual(message.Payload, result.Payload);
            }
        }

        [TestMethod]
        public async Task FindAuditMessagesByCriteriaAsync_ShouldNotReturnAuditMessagesBeforeGivenFromTimestamp()
        {
            var criteria = new AuditMessageCriteria
            {
                FromTimestamp = new DateTime(2018, 12, 10).Ticks
            };
        
            var result = await _target.FindAuditMessagesByCriteriaAsync(criteria);

            var auditMessages = result.ToList();
            Assert.AreEqual(2, auditMessages.Count);
            Assert.IsFalse(auditMessages.Any(m => m.CorrelationId == "msg1"), "Result should not contain message 1");
            Assert.IsFalse(auditMessages.Any(m => m.CorrelationId == "msg2"), "Result should not contain message 2");
        }

        [TestMethod]
        public async Task FindAuditMessagesByCriteriaAsync_ShouldNotReturnAuditMessagesAfterGivenToTimestamp()
        {
            var criteria = new AuditMessageCriteria
            {
                ToTimestamp = new DateTime(2018, 12, 10).Ticks
            };

            var result = await _target.FindAuditMessagesByCriteriaAsync(criteria);

            var auditMessages = result.ToList();
            Assert.AreEqual(4, auditMessages.Count);
            Assert.IsFalse(auditMessages.Any(m => m.CorrelationId == "msg5"), "Result should not contain message 5");
        }

        [TestMethod]
        public async Task FindAuditMessagesByCriteriaAsync_ShouldOnlyReturnAuditMessagesBetweenGivenFromAndToTimestamp()
        {
            var criteria = new AuditMessageCriteria
            {
                FromTimestamp = new DateTime(2018, 12, 1).Ticks,
                ToTimestamp = new DateTime(2018, 12, 10).Ticks
            };

            var result = await _target.FindAuditMessagesByCriteriaAsync(criteria);

            var auditMessages = result.ToList();
            Assert.AreEqual(3, auditMessages.Count);
            Assert.IsFalse(auditMessages.Any(m => m.CorrelationId == "msg1"), "Result should not contain message 1");
            Assert.IsFalse(auditMessages.Any(m => m.CorrelationId == "msg5"), "Result should not contain message 5");
        }

        [TestMethod]
        public async Task FindAuditMessagesByCriteriaAsync_ShouldOnlyReturnAuditMessagesWithGivenEventType()
        {
            var criteria = new AuditMessageCriteria
            {
                EventType = "Type1"
            };

            var result = await _target.FindAuditMessagesByCriteriaAsync(criteria);

            var auditMessages = result.ToList();
            Assert.AreEqual(3, auditMessages.Count);
            Assert.IsFalse(auditMessages.Any(m => m.CorrelationId == "msg1"), "Result should not contain message 1");
            Assert.IsFalse(auditMessages.Any(m => m.CorrelationId == "msg5"), "Result should not contain message 5");
        }

        [TestMethod]
        public async Task FindAuditMessagesByCriteriaAsync_ShouldOnlyReturnAuditMessagesMatchingRoutingKeyExpression()
        {
            var criteria = new AuditMessageCriteria
            {
                RoutingKeyExpression = "a.b.*"
            };

            var result = await _target.FindAuditMessagesByCriteriaAsync(criteria);

            var auditMessages = result.ToList();
            Assert.AreEqual(3, auditMessages.Count);
            Assert.IsFalse(auditMessages.Any(m => m.CorrelationId == "msg1"), "Result should not contain message 1");
            Assert.IsFalse(auditMessages.Any(m => m.CorrelationId == "msg3"), "Result should not contain message 3");
        }

        [TestMethod]
        public async Task FindAuditMessagesByCriteriaAsync_ShouldOnlyReturnAuditMessagesMatchingTheCriteria()
        {
            var criteria = new AuditMessageCriteria
            {
                FromTimestamp = new DateTime(2018, 12, 08).Ticks,
                ToTimestamp = new DateTime(2018, 12, 11).Ticks,
                EventType = "Type1",
                RoutingKeyExpression = "a.b.*"
            };

            var result = await _target.FindAuditMessagesByCriteriaAsync(criteria);

            var auditMessages = result.ToList();
            Assert.AreEqual(2, auditMessages.Count);
            Assert.IsFalse(auditMessages.Any(m => m.CorrelationId == "msg1"), "Result should not contain message 1");
            Assert.IsFalse(auditMessages.Any(m => m.CorrelationId == "msg3"), "Result should not contain message 3");
            Assert.IsFalse(auditMessages.Any(m => m.CorrelationId == "msg5"), "Result should not contain message 5");
        }
    }
}
