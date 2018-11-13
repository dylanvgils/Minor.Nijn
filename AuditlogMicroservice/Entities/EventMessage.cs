using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuditlogMicroservice.Entities
{
    public class EventMessage
    {
        public long Id { get; set; }
        public string RoutingKey { get; set; }
        public string Message { get; set; }
        public string EventType { get; set; }
        public long Timestamp { get; set; }
        public string CorrelationId { get; set; }
    }
}
