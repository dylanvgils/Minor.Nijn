using AuditlogMicroservice.DAL;
using AuditlogMicroservice.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuditlogMicroservice.EventListeners
{
    public class MessageEventListener
    {
        private IDataMapper<Event, long> _dataMapper;

        public MessageEventListener(IDataMapper<Event, long> dataMapper)
        {
            _dataMapper = dataMapper;
        }

        public void HandleEvent(Minor.Nijn.EventMessage message)
        {
            _dataMapper.Insert(new Event { RoutingKey = message.RoutingKey, Message = message.Message, Timestamp = message.Timestamp });
        }
    }
}
