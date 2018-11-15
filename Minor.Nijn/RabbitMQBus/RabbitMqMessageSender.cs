using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQMessageSender : IMessageSender
    {
        private readonly ILogger _log;

        public IModel Channel { get; private set; }
        private readonly IRabbitMQBusContext _context;

        private RabbitMQMessageSender() { }
        
        internal RabbitMQMessageSender(IRabbitMQBusContext context)
        {
            _context = context;
            Channel = context.Connection.CreateModel();

            _log = NijnLogging.CreateLogger<RabbitMQMessageSender>();
        }

        // TODO: logica adhv eventtype
        public void SendMessage(EventMessage message)
        {
            _log.LogInformation("Send message");

           Channel.BasicPublish(
               exchange: _context.ExchangeName,
               routingKey: message.RoutingKey,
               mandatory: false,
               basicProperties: Channel.CreateBasicProperties(),
               body: Encoding.UTF8.GetBytes(message.Message
           ));
        }

        public void Dispose()
        {
            Channel?.Dispose();
        }
    }
}
