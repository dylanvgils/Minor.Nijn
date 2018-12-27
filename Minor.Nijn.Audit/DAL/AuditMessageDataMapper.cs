using Microsoft.EntityFrameworkCore;
using Minor.Nijn.Audit.Entities;
using Minor.Nijn.Audit.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minor.Nijn.Audit.DAL
{
    public class AuditMessageDataMapper : IAuditMessageDataMapper
    {
        private readonly DbContextOptions<AuditContext> _options;

        public AuditMessageDataMapper(DbContextOptions<AuditContext> options)
        {
            _options = options;
        }

        public Task InsertAsync(AuditMessage item)
        {
            using (var context = new AuditContext(_options))
            {
                context.AuditMessages.Add(item);
                return context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<AuditMessage>> FindAuditMessagesByCriteriaAsync(AuditMessageCriteria criteria)
        {
            using (var context = new AuditContext(_options))
            {
                var query = context.AuditMessages.Where(m =>
                    (criteria.FromTimestamp == null || m.Timestamp >= criteria.FromTimestamp)
                    && (criteria.ToTimestamp == null || m.Timestamp <= criteria.ToTimestamp)
                    && (criteria.EventType == null || m.Type == criteria.EventType)
                    && (criteria.RoutingKeyExpression == null || TopicMatcher.IsMatch(new List<string> { criteria.RoutingKeyExpression }, m.RoutingKey))
                );

                return await query.ToListAsync();
            }
        }
    }
}