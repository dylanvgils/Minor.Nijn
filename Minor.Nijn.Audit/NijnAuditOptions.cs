using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Minor.Nijn.Audit.DAL;
using Minor.Nijn.WebScale;
using RabbitMQ.Client;
using System;
using System.Linq;

namespace Minor.Nijn.Audit
{
    public class NijnAuditOptions
    {
        public IServiceCollection ServiceCollection { get; set; }
        public DbContextOptions<AuditContext> DbContextOptions { get; set; }
        public IMicroserviceHostBuilder MicroserviceHostBuilder { get; set; }

        public NijnAuditOptions()
        {
            ServiceCollection = new ServiceCollection();
        }

        private void ConfigureServices()
        {
            ServiceCollection.AddLogging();
            ServiceCollection.TryAddSingleton(DbContextOptions);
            ServiceCollection.TryAddTransient<IAuditMessageDataMapper, AuditMessageDataMapper>();
            ServiceCollection.TryAddTransient<IEventReplayer, EventReplayer>();
        }

        private void ConfigureListeners()
        {
            MicroserviceHostBuilder.AddListener<AuditEventListener>();
            MicroserviceHostBuilder.AddListener<AuditCommandListener>();
        }

        private void CreateDatabase()
        {
            using (var context = new AuditContext(DbContextOptions))
            {
                context.Database.EnsureCreated();
            }
        }

        public void Configure()
        {
            if (ServiceCollection == null)
            {
                throw new ArgumentException("ServiceCollection should not be null");
            }

            if (ServiceCollection.All(s => s.ServiceType != typeof(IBusContext<IConnection>)))
            {
                throw new ArgumentException("IBusContext<IConnection> should be registered in the ServiceCollection");
            }

            if (MicroserviceHostBuilder == null)
            {
                throw new ArgumentException("MicroserviceHost should not be null");
            }

            if (DbContextOptions == null)
            {
                throw new ArgumentException("DbContextOptions should not be null");
            }

            ConfigureServices();
            ConfigureListeners();
            CreateDatabase();
        }

    }
}