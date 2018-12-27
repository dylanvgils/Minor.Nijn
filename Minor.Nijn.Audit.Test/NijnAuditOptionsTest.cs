using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Audit.DAL;
using Minor.Nijn.WebScale;
using Moq;
using RabbitMQ.Client;
using System;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Minor.Nijn.TestBus;

namespace Minor.Nijn.Audit.Test
{
    [TestClass]
    public class NijnAuditOptionsTest
    {
        private IServiceCollection _serviceCollection;
        private Mock<IMicroserviceHostBuilder> _microServiceHostBuildMock;

        private DbContextOptions<AuditContext> _dbContextOptions;
        private SqliteConnection _connection;

        private NijnAuditOptions _target;

        [TestInitialize]
        public void BeforeEach()
        {
            _serviceCollection = new ServiceCollection();
            _serviceCollection.AddSingleton<IBusContext<IConnection>>(new TestBusContextBuilder().CreateTestContext());

            _microServiceHostBuildMock = new Mock<IMicroserviceHostBuilder>(MockBehavior.Strict);

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _dbContextOptions = new DbContextOptionsBuilder<AuditContext>()
                .UseSqlite(_connection)
                .Options;

            _target = new NijnAuditOptions
            {
                ServiceCollection = _serviceCollection,
                MicroserviceHostBuilder = _microServiceHostBuildMock.Object,
                DbContextOptions = _dbContextOptions,
            };
        }

        [TestCleanup]
        public void AfterEach()
        {
            _connection.Dispose();
        }

        [TestMethod]
        public void Configure_ShouldConfigureNijnAudit()
        {
            _microServiceHostBuildMock.Setup(builder => builder.AddListener<AuditEventListener>())
                .Returns(_microServiceHostBuildMock.Object as MicroserviceHostBuilder);
            _microServiceHostBuildMock.Setup(builder => builder.AddListener<AuditCommandListener>())
                .Returns(_microServiceHostBuildMock.Object as MicroserviceHostBuilder);

            _target.Configure();

            _microServiceHostBuildMock.VerifyAll();

            Assert.IsTrue(_serviceCollection.Any(s => s.ServiceType == typeof(ILoggerFactory)), "Should contain ILoggerFactory");
            Assert.IsTrue(_serviceCollection.Any(s => s.ServiceType == typeof(DbContextOptions<AuditContext>)), "Should contain DbContextOptions<AuditContext>");
            Assert.IsTrue(_serviceCollection.Any(s => s.ServiceType == typeof(IBusContext<IConnection>)), "Should contain IBusContext<IConnection>");
            Assert.IsTrue(_serviceCollection.Any(s => s.ServiceType == typeof(IAuditMessageDataMapper)), "Should contain AuditLogDataMapper");
            Assert.IsTrue(_serviceCollection.Any(s => s.ServiceType == typeof(IEventReplayer)), "Should contain EventReplayer");
        }

        [TestMethod]
        public void Configure_ShouldThrowExceptionWhenServiceCollectionIsNull()
        {
            _target.ServiceCollection = null;

            var ex = Assert.ThrowsException<ArgumentException>(() => _target.Configure());
            Assert.AreEqual("ServiceCollection should not be null", ex.Message);
        }

        [TestMethod]
        public void Configure_ShouldThrowExceptionWhenMicroserviceHostBuilderIsNull()
        {
            _target.MicroserviceHostBuilder = null;

            var ex = Assert.ThrowsException<ArgumentException>(() => _target.Configure());
            Assert.AreEqual("MicroserviceHost should not be null", ex.Message);
        }

        [TestMethod]
        public void Configure_ShouldThrowExceptionWhenDbContextIsNull()
        {
            _target.DbContextOptions = null;

            var ex = Assert.ThrowsException<ArgumentException>(() => _target.Configure());
            Assert.AreEqual("DbContextOptions should not be null", ex.Message);
        }

        [TestMethod]
        public void Configure_ShouldThrowExceptionWhenRabbitMQConnectionIsNull()
        {
            _target.ServiceCollection = new ServiceCollection();

            var ex = Assert.ThrowsException<ArgumentException>(() => _target.Configure());
            Assert.AreEqual("IBusContext<IConnection> should be registered in the ServiceCollection", ex.Message);
        }
    }
}