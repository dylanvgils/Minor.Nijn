using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Audit.DAL;
using Minor.Nijn.Audit.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Minor.Nijn.Audit.Test
{
    [TestClass]
    public class EventMessageDataMapperTest
    {
        private SqliteConnection _connection;
        private DbContextOptions _options;

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
            }

            _target = new AuditMessageDataMapper(_options);
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
                Type = "Type",
                Timestamp = DateTime.Now.Ticks,
                Payload = "Payload"
            };

            await _target.InsertAsync(message);

            using (var context = new AuditContext(_options))
            {
                var result = context.Messages.SingleOrDefault(m => m.Id == 1);

                Assert.IsNotNull(result, "Result should not be null");
                Assert.AreEqual(message.RoutingKey, result.RoutingKey);
                Assert.AreEqual(message.Type, result.Type);
                Assert.AreEqual(message.Timestamp, result.Timestamp);
                Assert.AreEqual(message.Payload, result.Payload);
            }
        }
    }
}
