using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQMessageSender : IMessageSender
    {
        private readonly ILogger _log;

        private RabbitMQBusContext Context { get; set; }

        public IModel Channel { get; private set; }

        public RabbitMQMessageSender(RabbitMQBusContext context)
        {
            Context = context;
            Channel = context.Connection.CreateModel();

            _log = NijnLogging.CreateLogger<RabbitMQMessageSender>();
        }

        // TODO: logica adhv eventtype
        public void SendMessage(EventMessage message)
        {
            _log.LogInformation("Send message");

           Channel.BasicPublish(exchange: Context.ExchangeName,
                                   routingKey: message.RoutingKey,
                                   mandatory: false,
                                   basicProperties: Channel.CreateBasicProperties(),
                                   body: Encoding.UTF8.GetBytes(message.Message));
        }

        public void Dispose()
        {
            Channel?.Dispose();
        }
    }
}
