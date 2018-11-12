using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQMessageReceiver : IMessageReceiver
    {
        public string QueueName => throw new NotImplementedException();
        public IEnumerable<string> TopicExpressions => throw new NotImplementedException();

        public RabbitMQMessageReceiver(RabbitMQBusContext context, 
                        string queueName, IEnumerable<string> topicExpressions)
        {
            throw new NotImplementedException();
        }

        public void DeclareQueue()
        {
            throw new NotImplementedException();
        }

        public void StartReceivingMessages(EventMessageReceivedCallback Callback)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
