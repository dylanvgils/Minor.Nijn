using Minor.Nijn.Audit.Entities;
using System.Threading.Tasks;

namespace Minor.Nijn.Audit.DAL
{
    public interface IAuditMessageDataMapper
    {
        Task InsertAsync(AuditMessage item);
    }
}