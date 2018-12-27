using Microsoft.EntityFrameworkCore;
using Minor.Nijn.Audit.DAL;
using Minor.Nijn.WebScale;
using System;

namespace Minor.Nijn.Audit.Helpers
{
    public static class MicroserviceHostBuilderExtensions
    {
        public static IMicroserviceHostBuilder WithNijnAudit(this IMicroserviceHostBuilder hostBuilder, DbContextOptions<AuditContext> dbContextOptions)
        {
            if (dbContextOptions == null)
            {
                throw new ArgumentException("dbContextOptions should not be null");
            }

            var builder = (MicroserviceHostBuilder) hostBuilder;

            var options = new NijnAuditOptions
            {
                MicroserviceHostBuilder = hostBuilder,
                DbContextOptions = dbContextOptions,
                ServiceCollection = builder.ServiceCollection,
            };

            options.Configure();
            return hostBuilder;
        }
    }
}