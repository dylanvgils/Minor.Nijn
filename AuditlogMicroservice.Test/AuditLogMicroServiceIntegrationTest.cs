using AuditlogMicroservice.DAL;
using AuditlogMicroservice.Entities;
using AuditlogMicroservice.EventListeners;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AuditlogMicroservice.Test
{
    [TestClass]
    public class AuditLogMicroServiceIntegrationTest
    {
        SqliteConnection connection;
        DbContextOptions<EventContext> options;
        IDataMapper<Event, long> dataMapper;

        [TestInitialize]
        public void Initialize()
        {
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            options = new DbContextOptionsBuilder<EventContext>()
                            .UseSqlite(connection)
                            .Options;
            using (var context = new EventContext(options))
            {
                context.Database.EnsureCreated();
            }
            dataMapper = new EventDataMapper(options);
        }

        [TestMethod]
        public void MyTestMethod()
        {
            var context = new TestBusContextBuilder().CreateTestContext();
            var sender = context.CreateMessageSender();
            var receiver = context.CreateMessageReceiver("TestQueue", new List<string> { "a.b.c" });
            receiver.DeclareQueue();

            var target = new MessageEventListener(dataMapper);
            receiver.StartReceivingMessages(target.HandleEvent);
            sender.SendMessage(new Minor.Nijn.EventMessage("a.b.c", "TestMessage", timestamp: 1234));

            using(var dbContext = new EventContext(options))
            {
                Event resultEvent = dbContext.Messages.SingleOrDefault(e => e.Message == "TestMessage");
                Assert.IsNotNull(resultEvent);
            }
        }
    }
}
