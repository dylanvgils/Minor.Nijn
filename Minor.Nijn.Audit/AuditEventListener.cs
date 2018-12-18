using Minor.Nijn.Audit.DAL;
using Minor.Nijn.Audit.Entities;
using Minor.Nijn.WebScale.Attributes;
using System.Threading.Tasks;

namespace Minor.Nijn.Audit
{
    [EventListener(Constants.AuditEventQueueName)]
    public class AuditEventListener
    {
        private readonly IAuditMessageDataMapper _dataMapper;

        public AuditEventListener(IAuditMessageDataMapper dataMapper)
        {
            _dataMapper = dataMapper;
        }

        [Topic(Constants.AuditEventPattern)]
        public async Task HandleEvents(EventMessage message)
        {
            var result = new AuditMessage
            {
                RoutingKey = message.RoutingKey,
                Type = message.Type,
                Timestamp = message.Timestamp,
                Payload = message.Message,
            };

            await _dataMapper.InsertAsync(result);
        }
    }
}
