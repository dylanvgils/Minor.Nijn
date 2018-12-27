using Minor.Nijn.Audit.Entities;
using Minor.Nijn.Audit.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Minor.Nijn.Audit.DAL
{
    public interface IAuditMessageDataMapper
    {
        Task InsertAsync(AuditMessage item);

        Task<IEnumerable<AuditMessage>> FindAuditMessagesByCriteriaAsync(AuditMessageCriteria criteria);
    }
}