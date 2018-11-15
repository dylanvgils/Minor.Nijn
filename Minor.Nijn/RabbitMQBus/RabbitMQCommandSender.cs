using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandSender : ICommandSender
    {
        public IModel Channel { get; private set; }

        private readonly IRabbitMQBusContext _context;

        private RabbitMQCommandSender() { }

        internal RabbitMQCommandSender(IRabbitMQBusContext context)
        {
            _context = context;

            Channel = context.Connection.CreateModel();
        }

        public Task<CommandMessage> SendCommandAsync(CommandMessage request)
        {
            var replyQueue = _context.CreateCommandReceiver();
            replyQueue.DeclareCommandQueue();

            var props = Channel.CreateBasicProperties();
            props.ReplyTo = replyQueue.QueueName;

            var task = Task.Run(() => {
                var flag = new ManualResetEvent(false);

                CommandMessage response = null;
                replyQueue.StartReceivingCommands((args, sender) => {
                    response = args;
                    flag.Set();
                });

                flag.WaitOne(5000);

                return response;
            });

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
        
        public void Dispose()
        {
            Channel?.Dispose();
        }
    }
}