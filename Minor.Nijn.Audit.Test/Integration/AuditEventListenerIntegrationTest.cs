using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Audit.DAL;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Minor.Nijn.Audit.Test.Integration
{
    [TestClass]
    public class AuditEventListenerIntegrationTest
    {
        private SqliteConnection _connection;
        private DbContextOptions<AuditContext> _options;

        private AuditEventListener _target;

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

            var dataMapper = new AuditMessageDataMapper(_options);

            _target = new AuditEventListener(dataMapper);
        }

        [TestCleanup]
        public void AfterEach()
        {
            _connection.Dispose();
        }

        [TestMethod]
        public async Task ShouldAddIncomingEventsToDatabase()
        {
            var message = new EventMessage(
                routingKey: "RoutingKey",
                message: "Message",
                type: "type",
                timestamp: DateTime.Now.Ticks,
                correlationId: "CorrelationId"
            );

            await _target.HandleEvents(message);

            using (var context = new AuditContext(_options))
            {
                var result = context.AuditMessages.SingleOrDefault(m => m.Id == 1);

                Assert.IsNotNull(result, "Result should not be null");
                Assert.AreEqual(message.RoutingKey, result.RoutingKey);
                Assert.AreEqual(message.Type, result.Type);
                Assert.AreEqual(message.Timestamp, result.Timestamp);
                Assert.AreEqual(message.Message, result.Payload);
            }
        }
    }
}