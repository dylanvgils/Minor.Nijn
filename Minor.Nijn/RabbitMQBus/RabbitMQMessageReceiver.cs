using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQMessageReceiver : IMessageReceiver
    {
        private RabbitMQBusContext Context;
        public string QueueName { get; set; }
        public IEnumerable<string> TopicExpressions { get; set; }
        private IModel Channel { get; set; }

        public RabbitMQMessageReceiver(RabbitMQBusContext context, 
                        string queueName, IEnumerable<string> topicExpressions)
        {
            Context = context;
            QueueName = queueName;
            TopicExpressions = topicExpressions;
            Channel = Context.Connection.CreateModel();
        }

        public void DeclareQueue()
        {
            Channel.QueueDeclare(queue: QueueName,
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);


            foreach (var topic in TopicExpressions)
            {
                Channel.QueueBind(queue: QueueName,
                            exchange: Context.ExchangeName,
                            routingKey: topic);
            }    
        }

        public void StartReceivingMessages(EventMessageReceivedCallback Callback)
        {
            var consumer = new EventingBasicConsumer(Channel);

            consumer.Received += (model, ea) =>
            {
                Callback.Invoke(new EventMessage(routingKey: ea.RoutingKey,
                                                message: Encoding.UTF8.GetString(ea.Body),
                                                eventType: null,
                                                timestamp: 0,
                                                correlationId: null));
            };
        }

        public void Dispose()
        {
            Channel?.Dispose();
        }
    }
}
