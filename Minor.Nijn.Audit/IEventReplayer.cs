using Minor.Nijn.Audit.Entities;

namespace Minor.Nijn.Audit
{
    public interface IEventReplayer
    {
        string ExchangeName { get; }
        bool ExchangeDeclared { get; }

        void DeclareExchange(string exchangeName);

        void ReplayAuditMessage(AuditMessage message);
    }
}