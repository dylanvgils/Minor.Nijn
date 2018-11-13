using AuditlogMicroservice.DAL;
using AuditlogMicroservice.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AuditlogMicroservice.Test
{
    [TestClass]
    public class EventMessageDataMapperTest
    {
        SqliteConnection connection;
        DbContextOptions<EventMessageContext> options;

        [TestInitialize]
        public void Initialize()
        {
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            options = new DbContextOptionsBuilder<EventMessageContext>()
                            .UseSqlite(connection)
                            .Options;
            using (var context = new EventMessageContext(options))
            {
                context.Database.EnsureCreated();
            }
        }

        [TestMethod]
        public void Insert_ShouldInsertEventMessage()
        {

            var target = new EventMessageDataMapper(options);
            var message = new EventMessage { Id = 1, RoutingKey = "a.b.c", Message = "TestBericht" };

            target.Insert(message);

            using (var context = new EventMessageContext(options))
            {
                EventMessage resultEvent = context.Messages.SingleOrDefault(m => m.Id == 1);
                Assert.IsNotNull(resultEvent);
            }
        }
    }
}
