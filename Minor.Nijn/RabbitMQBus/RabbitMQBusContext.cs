using RabbitMQ.Client;
using System.Collections.Generic;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQBusContext : IRabbitMQBusContext
    {        
        public IConnection Connection { get; }
        public string ExchangeName { get; }

        internal RabbitMQBusContext(IConnection connection, string exchangeName)
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
        
        public ICommandSender CreateCommandSender()
        {
            return new RabbitMQCommandSender(this);
        }

        public ICommandReceiver CreateCommandReceiver(string queueName)
        {
            return new RabbitMQCommandReceiver(this, queueName);
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}
