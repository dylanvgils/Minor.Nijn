using RabbitMQ.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQMessageSender : IMessageSender
    {
        private RabbitMQBusContext Context { get; set; }

        private IModel Channel { get; set; }
        public RabbitMQMessageSender(RabbitMQBusContext context)
        {
            Context = context;
            Channel = context.Connection.CreateModel();
        }

        // TODO: logica adhv eventtype
        public void SendMessage(EventMessage message)
        {
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
