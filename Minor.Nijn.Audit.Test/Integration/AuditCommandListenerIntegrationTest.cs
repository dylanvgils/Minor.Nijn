using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Audit.DAL;
using Minor.Nijn.Audit.Entities;
using Minor.Nijn.Audit.Models;
using Minor.Nijn.TestBus;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IBasicProperties = RabbitMQ.Client.IBasicProperties;

namespace Minor.Nijn.Audit.Test.Integration
{
    [TestClass]
    public class AuditCommandListenerIntegrationTest
    {
        private ITestBusContext _busContext;
        private Mock<IModel> _channelMock;

        private SqliteConnection _connection;
        private DbContextOptions<AuditContext> _options;

        private AuditCommandListener _target;

        [TestInitialize]
        public void BeforeEach()
        {
            var loggerFactory = new LoggerFactory();

            _channelMock = new Mock<IModel>(MockBehavior.Strict);
            var busConnectionMock = new Mock<IConnection>(MockBehavior.Strict);
            busConnectionMock.Setup(conn => conn.CreateModel()).Returns(_channelMock.Object);

            _busContext = new TestBusContextBuilder()
                .WithMockConnection(busConnectionMock.Object)
                .CreateTestContext();

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

            var dataMapper = new AuditMessageDataMapper(_options);
            var replayer = new EventReplayer(_busContext, loggerFactory);

            _target = new AuditCommandListener(dataMapper, replayer, loggerFactory);
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
        public async Task ShouldReturnReplayEventsCommandResult()
        {
            var command = new ReplayEventsCommand(Constants.AuditCommandQueueName)
            {
                ExchangeName = "ReplyExchangeName",
                EventType = "Type2",
                FromTimestamp = new DateTime(2018, 12, 01).Ticks
            };

            var propsMock = new Mock<IBasicProperties>(MockBehavior.Strict);
            propsMock.SetupSet(props => props.CorrelationId = "msg5");
            propsMock.SetupSet(props => props.Timestamp = It.IsAny<AmqpTimestamp>());
            propsMock.SetupSet(props => props.Type = command.EventType);

            _channelMock.Setup(chan => chan.ExchangeDeclare(command.ExchangeName, Constants.ReplayerExchangeType, false, true, null));
            _channelMock.Setup(chan => chan.CreateBasicProperties()).Returns(propsMock.Object);
            _channelMock.Setup(chan => chan.BasicPublish(command.ExchangeName, "a.b.c", false, It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()));

            var result = await _target.HandleReplayEventCommands(command);
            Thread.Sleep(250);

            propsMock.VerifyAll();
            _channelMock.VerifyAll();

            Assert.IsFalse(result.StartTimestamp == 0, "StartTimestamp should not be null");
            Assert.AreEqual(command.ExchangeName, result.ExchangeName);
            Assert.AreEqual(1, result.NumberOfEvents);
        }
    }
}