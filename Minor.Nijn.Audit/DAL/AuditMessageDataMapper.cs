using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Minor.Nijn.Audit.Entities;

namespace Minor.Nijn.Audit.DAL
{
    internal class AuditMessageDataMapper : IAuditMessageDataMapper
    {
        private readonly DbContextOptions _options;

        public AuditMessageDataMapper(DbContextOptions options)
        {
            _options = options;
        }

        public Task InsertAsync(AuditMessage item)
        {
            using (var context = new AuditContext(_options))
            {
                context.Messages.Add(item);
                return context.SaveChangesAsync();
            }
        }
    }
}