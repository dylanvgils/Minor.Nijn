using RabbitMQ.Client;
using System;
using System.Text;
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

            CommandReceivedCallback callback = commandMessage =>
            {
                // Do some action and put response in reply message
                var replyMessage = new CommandMessage("Test Reply message", "type", "id");

                // Send reply with reply sender
                var replySender = new RabbitMQCommandReplySender();
                replySender.SendCommandReply(replyMessage);
            };

            replyQueue.StartReceivingCommands(callback);

            var props = Channel.CreateBasicProperties();

            Channel.BasicPublish(
                exchange: "",
                routingKey: request.ReplyTo,
                mandatory: false,
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(request.Message)
            );

            //Channel.BasicAck(
            //    deliveryTag: ea.DeliveryTag,
            //    multiple: false
            //);

            throw new System.NotImplementedException();
        }
        
        public void Dispose()
        {
            Channel?.Dispose();
        }
    }
}