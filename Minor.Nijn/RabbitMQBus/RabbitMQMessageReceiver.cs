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
        public IModel Channel { get; private set; }

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
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);


            foreach (var topic in TopicExpressions)
            {
                Channel.QueueBind(queue: QueueName,
                            exchange: Context.ExchangeName,
                            routingKey: topic, 
                            arguments: null);
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

            Channel.BasicConsume(queue: QueueName,
                            autoAck: true,
                            consumer: consumer);
        }

        public void Dispose()
        {
            Channel?.Dispose();
        }
    }
}
