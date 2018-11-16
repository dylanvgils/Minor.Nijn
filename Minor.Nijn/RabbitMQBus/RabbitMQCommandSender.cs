using System;
using RabbitMQ.Client;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandSender : ICommandSender
    {
        private readonly ILogger _log;
        private readonly EventingBasicConsumerFactory _eventingBasicConsumerFactory;
        
        public IModel Channel { get; }

        private RabbitMQCommandSender() { }

        internal RabbitMQCommandSender(IRabbitMQBusContext context, EventingBasicConsumerFactory factory) : this(context)
        {
            _eventingBasicConsumerFactory = factory;
        }
        
        internal RabbitMQCommandSender(IRabbitMQBusContext context)
        {
            Channel = context.Connection.CreateModel();
            _eventingBasicConsumerFactory = new EventingBasicConsumerFactory();

            _log = NijnLogging.CreateLogger<RabbitMQCommandSender>();
        }

        public Task<CommandMessage> SendCommandAsync(CommandMessage request)
        {
            _log.LogInformation("Sending command to {0}", request.RoutingKey);
            string replyQueueName = Channel.QueueDeclare().QueueName;;

            var props = Channel.CreateBasicProperties();
            props.ReplyTo = replyQueueName;

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

        private Task<CommandMessage> SubscribeToResponseQueue(string replyQueueName, string correlationId)
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

        private Task<CommandMessage> StartResponseAwaiterTask(EventingBasicConsumer consumer, string correlationId)
        {               
            return Task.Run(() => {
                var flag = new ManualResetEvent(false);

                CommandMessage response = null;
                consumer.Received += (sender, args) => {
                    _log.LogInformation("Received response message, with id {0}", args.BasicProperties.MessageId);
                    if (args.BasicProperties.CorrelationId != correlationId)
                    {
                        _log.LogDebug("Received response with wrong correlationId, id was {0}, expected {1}", 
                            args.BasicProperties.CorrelationId, 
                            correlationId
                        );
                        
                        return;
                    }
                  
                    string body = Encoding.UTF8.GetString(args.Body);
                    
                    response = new CommandMessage(
                        body,
                        args.BasicProperties.Type,
                        args.BasicProperties.CorrelationId
                    );

                    flag.Set();
                };

                flag.WaitOne(5000);

                return response;
            });
        }
        
        public void Dispose()
        {
            Channel?.Dispose();
        }
    }
}