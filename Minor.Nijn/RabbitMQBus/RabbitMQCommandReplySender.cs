using RabbitMQ.Client;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandReplySender : ICommandReplySender
    {
        private readonly IRabbitMQBusContext _context;
        private readonly string _replyTo;

        public IModel Channel { get; private set; }

        public RabbitMQCommandReplySender(IRabbitMQBusContext context, string replyTo)
        {
            _context = context;
            _replyTo = replyTo;

            Channel = context.Connection.CreateModel();
        }

        public void SendCommandReply(CommandMessage reply)
        {
            Channel.BasicPublish(
                exchange: "",
                routingKey: _replyTo,
                mandatory: false,
                basicProperties: null,
                body: Encoding.UTF8.GetBytes(reply.Message)
            );

            Channel.BasicAck(
                deliveryTag: 0,
                multiple: false
            );
        }

        public void Dispose()
        {
            Channel?.Dispose();
        }
    }
}
