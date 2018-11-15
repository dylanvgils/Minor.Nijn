using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandReceiver : ICommandReceiver
    {
        public string QueueName { get; }
        public IModel Channel { get; private set; }

        private readonly IRabbitMQBusContext _context;

        private RabbitMQCommandReceiver() { }

        internal RabbitMQCommandReceiver(IRabbitMQBusContext context, string queueName)
        {
            _context = context;
            QueueName = queueName;

            Channel = context.Connection.CreateModel();
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
            var consumer = new EventingBasicConsumer(Channel);

            consumer.Received += (model, args) =>
            {
                string message = Encoding.UTF8.GetString(args.Body);

                callback(new CommandMessage(
                    message: message,
                    type: args.BasicProperties.Type,
                    correlationId: args.BasicProperties.CorrelationId
                ), _context.CreateCommandReplySender(args.BasicProperties.ReplyTo));
            };

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
        
        public void Dispose()
        {
            Channel?.Dispose();
        }
    }
}