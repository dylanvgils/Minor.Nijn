using System;
using RabbitMQ.Client;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandSender : ICommandSender
    {
        public IModel Channel { get; }

        private readonly IRabbitMQBusContext _context;

        private RabbitMQCommandSender() { }

        internal RabbitMQCommandSender(IRabbitMQBusContext context)
        {
            _context = context;

            Channel = context.Connection.CreateModel();
        }

        public Task<CommandMessage> SendCommandAsync(CommandMessage request)
        {
            string replyQueueName = Channel.QueueDeclare().QueueName;;

            var props = Channel.CreateBasicProperties();
            props.ReplyTo = replyQueueName;

            var task = SubscribeToResponseQueue(replyQueueName);

            Channel.BasicPublish(
                exchange: "",
                routingKey: request.RoutingKey,
                mandatory: false,
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(request.Message)
            );

            Channel.BasicAck(
                deliveryTag: 0,
                multiple: false
            );

            return task;
        }

        private Task<CommandMessage> SubscribeToResponseQueue(string replyQueueName)
        {
            var consumer = new EventingBasicConsumer(Channel);
            var task = StartResponseAwaiterTask(consumer);
            
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

        private Task<CommandMessage> StartResponseAwaiterTask(EventingBasicConsumer consumer)
        {               
            return Task.Run(() => {
                var flag = new ManualResetEvent(false);

                CommandMessage response = null;
                consumer.Received += (sender, args) => {
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