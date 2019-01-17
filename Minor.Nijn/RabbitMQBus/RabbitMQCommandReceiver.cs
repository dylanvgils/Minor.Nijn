using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandReceiver : ICommandReceiver
    {   
        private readonly ILogger _logger;
        private readonly EventingBasicConsumerFactory _eventingBasicConsumerFactory;
        private bool _disposed;

        public string QueueName { get; }
        public IModel Channel { get; }
        private bool _queueDeclared;

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

            _logger = NijnLogger.CreateLogger<RabbitMQCommandReceiver>();
        }

        public void DeclareCommandQueue()
        {
            CheckDisposed();
            CheckQueueAlreadyDeclared();

            _logger.LogInformation("Declaring command queue with name: {0}", QueueName);
            
            Channel.QueueDeclare(
                queue: QueueName, 
                durable: false,
                exclusive: false,
                autoDelete: true, 
                arguments: null
            );

            Channel.BasicQos(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false
            );

            _queueDeclared = true;
        }

        public void StartReceivingCommands(CommandReceivedCallback callback)
        {
            CheckDisposed();
            CheckQueueDeclared();

            _logger.LogInformation("Start listening for commands on queue: {0}", QueueName);

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
                _logger.LogInformation("Received command with correlationId: {0}", args.BasicProperties.CorrelationId);
                string requestBody = Encoding.UTF8.GetString(args.Body);
                
                var replyMessage = callback(new RequestCommandMessage(
                    message: requestBody,
                    type: args.BasicProperties.Type,
                    correlationId: args.BasicProperties.CorrelationId,
                    timestamp: args.BasicProperties.Timestamp.UnixTime
                ));

                PublishResponse(args, replyMessage);
            };

            return consumer;
        }

        private void PublishResponse(BasicDeliverEventArgs args, CommandMessage replyMessage)
        {
            _logger.LogInformation("Sending command reply to: {0}", args.BasicProperties.ReplyTo);
            
            var replyProps = Channel.CreateBasicProperties();
            replyProps.CorrelationId = args.BasicProperties.CorrelationId;
            replyProps.Type = replyMessage.Type ?? "";
            replyProps.Timestamp = replyMessage.Timestamp == 0
                ? new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                : new AmqpTimestamp(replyMessage.Timestamp);

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

        private void CheckQueueAlreadyDeclared()
        {
            if (!_queueDeclared) return;
            _logger.LogDebug("Queue with name: {0} is already declared", QueueName);
            throw new BusConfigurationException($"Queue with name: {QueueName} is already declared");
        }

        private void CheckQueueDeclared()
        {
            if (_queueDeclared) return;
            _logger.LogDebug("Queue with name: {0} is not declared", QueueName);
            throw new BusConfigurationException($"Queue with name: {QueueName} is not declared");
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RabbitMQCommandReceiver()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Channel?.Dispose();
            }

            _disposed = true;
        }
    }
}