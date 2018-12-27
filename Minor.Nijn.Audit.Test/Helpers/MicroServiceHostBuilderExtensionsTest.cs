using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Audit.DAL;
using Minor.Nijn.Audit.Helpers;
using Minor.Nijn.WebScale;
using Minor.Nijn.WebScale.Helpers;
using Moq;
using RabbitMQ.Client;
using System;
using System.Linq;

namespace Minor.Nijn.Audit.Test.Helpers
{
    [TestClass]
    public class MicroServiceHostBuilderExtensionsTest
    {
        private ServiceCollection _serviceCollection;
        private Mock<IBusContext<IConnection>> _nijnContextMock;

        private DbContextOptions<AuditContext> _dbContextOptions;
        private SqliteConnection _connection;

        private MicroserviceHostBuilder _target;

        [TestInitialize]
        public void BeforeEach()
        {
            _nijnContextMock = new Mock<IBusContext<IConnection>>();

            _serviceCollection = new ServiceCollection();
            _serviceCollection.AddSingleton(_nijnContextMock.Object);

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _dbContextOptions = new DbContextOptionsBuilder<AuditContext>()
                .UseSqlite(_connection)
                .Options;

            _target = _serviceCollection.AddNijnWebScale(options =>
            {
                options.WithContext(_nijnContextMock.Object);
            });
        }

        [TestCleanup]
        public void AfterEach()
        {
            _connection.Dispose();
        }

        [TestMethod]
        public void WithNijnAudit_ShouldRegisterItSelfToTheMicroserviceHostBuilder()
        {
            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            _nijnContextMock.SetupGet(ctx => ctx.Connection).Returns(connectionMock.Object);

            _target.WithNijnAudit(_dbContextOptions);

            Assert.IsTrue(_serviceCollection.Any(s => s.ServiceType == typeof(ILoggerFactory)), "Should contain ILoggerFactory");
            Assert.IsTrue(_serviceCollection.Any(s => s.ServiceType == typeof(DbContextOptions<AuditContext>)), "Should contain DbContextOptions<AuditContext>");
            Assert.IsTrue(_serviceCollection.Any(s => s.ServiceType == typeof(IBusContext<IConnection>)), "Should contain IBusContext<IConnection>");
            Assert.IsTrue(_serviceCollection.Any(s => s.ServiceType == typeof(IAuditMessageDataMapper)), "Should contain AuditLogDataMapper");
            Assert.IsTrue(_serviceCollection.Any(s => s.ServiceType == typeof(IEventReplayer)), "Should contain EventReplayer");
        }

        [TestMethod]
        public void WithNijnAudit_ShouldThrowExceptionWhenDbContextOptionsIsNull()
        {
            Action action = () => { _target.WithNijnAudit(null); };

            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual("dbContextOptions should not be null", ex.Message);
        }
    }
}