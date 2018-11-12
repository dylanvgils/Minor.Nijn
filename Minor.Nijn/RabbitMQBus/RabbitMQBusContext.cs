using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQBusContext : IBusContext<IConnection>
    {      
        public IConnection Connection { get; set; }
        public string ExchangeName { get; set; }

        public RabbitMQBusContext(IConnection connection, string exchangeName)
        {
            Connection = connection;
            ExchangeName = exchangeName;
        }

        public IMessageSender CreateMessageSender()
        {
            return new RabbitMQMessageSender(this);
        }

        public IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions)
        {
            return new RabbitMQMessageReceiver(this, queueName, topicExpressions);
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}
