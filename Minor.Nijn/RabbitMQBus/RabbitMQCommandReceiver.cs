using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandReceiver : ICommandReceiver
    {
        private readonly ILogger _log;
        private readonly EventingBasicConsumerFactory _eventingBasicConsumerFactory;
        
        public string QueueName { get; }
        public IModel Channel { get; }

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

            _log = NijnLogging.CreateLogger<RabbitMQCommandReceiver>();
        }

        public void DeclareCommandQueue()
        {
            _log.LogInformation("Declaring command queue with name: {0}", QueueName);
            
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
            _log.LogInformation("Start listening for commands on queue: {0}", QueueName);
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
                _log.LogInformation("Received command with id: {0}", args.BasicProperties.MessageId);
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
            _log.LogInformation("Sending command reply to: {0}", args.BasicProperties.ReplyTo);
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