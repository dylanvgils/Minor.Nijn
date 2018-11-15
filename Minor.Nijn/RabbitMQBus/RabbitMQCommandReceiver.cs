using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandReceiver : ICommandReceiver
    {
        public string QueueName { get; }
        public IModel Channel { get; }

        private readonly EventingBasicConsumerFactory _eventingBasicConsumerFactory;

        private RabbitMQCommandReceiver() { }

        internal RabbitMQCommandReceiver(
            IRabbitMQBusContext context,
            string queueName,
            EventingBasicConsumerFactory factory
        ) : this(context, queueName)
        {
            _eventingBasicConsumerFactory = factory;
        }
        
        internal RabbitMQCommandReceiver(IRabbitMQBusContext context, string queueName)
        {
            QueueName = queueName;
            Channel = context.Connection.CreateModel();
            _eventingBasicConsumerFactory = new EventingBasicConsumerFactory();
        }

        public void DeclareCommandQueue()
        {
            Channel.QueueDeclare(
                queue: QueueName, 
                durable: false,
                exclusive: false,
                autoDelete: false, 
                arguments: null
            );

            Channel.BasicQos(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false
            );
        }

        public void StartReceivingCommands(CommandReceivedCallback callback)
        {
            var consumer = CreateBasicConsumer(callback);

            Channel.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumerTag: "",
                noLocal: false,
                exclusive: false,
                arguments: null,
                consumer: consumer
            );
        }

        private EventingBasicConsumer CreateBasicConsumer(CommandReceivedCallback callback)
        {
            var consumer = _eventingBasicConsumerFactory.CreateEventingBasicConsumer(Channel);
            
            consumer.Received += (model, args) =>
            {   
                string requestBody = Encoding.UTF8.GetString(args.Body);
                
                var replyMessage = callback(new CommandMessage(
                    message: requestBody,
                    type: args.BasicProperties.Type,
                    correlationId: args.BasicProperties.CorrelationId
                ));
                
                PublishResponse(args, replyMessage);
            };

            return consumer;
        }

        private void PublishResponse(BasicDeliverEventArgs args, CommandMessage replyMessage)
        {
            var replyProps = Channel.CreateBasicProperties();
            replyProps.CorrelationId = args.BasicProperties.CorrelationId;
            
            Channel.BasicPublish(
                exchange: "",
                routingKey: args.BasicProperties.ReplyTo,
                mandatory: false,
                basicProperties: replyProps,
                body: Encoding.UTF8.GetBytes(replyMessage.Message)
            );
            
            Channel.BasicAck(
                deliveryTag: args.DeliveryTag,
                multiple: false
            );
        }
        
        public void Dispose()
        {
            Channel?.Dispose();
        }
    }
}