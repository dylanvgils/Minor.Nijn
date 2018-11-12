using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQBusContext : IBusContext<IConnection>
    {
        public IConnection Connection => throw new NotImplementedException();
        public string ExchangeName => throw new NotImplementedException();

        public RabbitMQBusContext(IConnection connection, string exchangeName)
        {
            throw new NotImplementedException();
            // TODO
        }

        public IMessageSender CreateMessageSender()
        {
            throw new NotImplementedException();
        }

        public IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
