using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandSender : ICommandSender
    {
        private readonly ILogger _logger;
        private readonly EventingBasicConsumerFactory _eventingBasicConsumerFactory;
        private bool _disposed;

        public IModel Channel { get; }

        internal RabbitMQCommandSender(IRabbitMQBusContext context, EventingBasicConsumerFactory factory) : this(context)
        {
            _eventingBasicConsumerFactory = factory;
        }
        
        internal RabbitMQCommandSender(IRabbitMQBusContext context)
        {
            Channel = context.Connection.CreateModel();
            _eventingBasicConsumerFactory = new EventingBasicConsumerFactory();

            _logger = NijnLogger.CreateLogger<RabbitMQCommandSender>();
        }

        public Task<ResponseCommandMessage> SendCommandAsync(RequestCommandMessage request)
        {
            CheckDisposed();

            _logger.LogInformation("Sending command to {0}", request.RoutingKey);
            string replyQueueName = Channel.QueueDeclare().QueueName;

            var props = Channel.CreateBasicProperties();
            props.ReplyTo = replyQueueName;
            props.CorrelationId = request.CorrelationId;
            props.Type = request.Type;
            props.Timestamp = request.Timestamp == 0
                ? new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                : new AmqpTimestamp(request.Timestamp);

            var task = SubscribeToResponseQueue(replyQueueName, request.CorrelationId);

            Channel.BasicPublish(
                exchange: "",
                routingKey: request.RoutingKey,
                mandatory: false,
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(request.Message)
            );

            return task;
        }

        private Task<ResponseCommandMessage> SubscribeToResponseQueue(string replyQueueName, string correlationId)
        {
            var consumer = _eventingBasicConsumerFactory.CreateEventingBasicConsumer(Channel);
            var task = StartResponseAwaiterTask(consumer, correlationId);
            
            Channel.BasicConsume(
                queue: replyQueueName,
                autoAck: true,
                consumerTag: "",
                noLocal: false,
                exclusive: false,
                arguments: null,
                consumer: consumer
            );

            return task;
        }

        private Task<ResponseCommandMessage> StartResponseAwaiterTask(EventingBasicConsumer consumer, string correlationId)
        {               
            return Task.Run(() => {
                var flag = new ManualResetEvent(false);

                ResponseCommandMessage response = null;
                consumer.Received += (sender, args) => {
                    _logger.LogInformation("Received response message, with correlationId {0}", args.BasicProperties.CorrelationId);
                    if (args.BasicProperties.CorrelationId != correlationId)
                    {
                        _logger.LogDebug("Received response with wrong correlationId, id was {0}, expected {1}", 
                            args.BasicProperties.CorrelationId, 
                            correlationId
                        );
                        
                        return;
                    }
                  
                    string body = Encoding.UTF8.GetString(args.Body);
                    
                    response = new ResponseCommandMessage(
                        message: body,
                        type: args.BasicProperties.Type,
                        correlationId: args.BasicProperties.CorrelationId,
                        timestamp: args.BasicProperties.Timestamp.UnixTime
                    );

                    flag.Set();
                };

                bool isSet = flag.WaitOne(Constants.CommandResponseTimeoutMs);
                if (!isSet)
                {
                    throw new TimeoutException($"No response received after {Constants.CommandResponseTimeoutMs / 1000} seconds");
                }

                return response;
            });
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

        ~RabbitMQCommandSender()
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