using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQMessageSender : IMessageSender
    {
        public RabbitMQMessageSender(RabbitMQBusContext context)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(EventMessage message)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
