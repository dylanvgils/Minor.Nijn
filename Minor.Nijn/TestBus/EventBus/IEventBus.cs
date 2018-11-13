using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.TestBus.EventBus
{
    internal interface IEventBus : IBus<EventMessage>
    {
        EventBusQueue DeclareQueue(string queueName, IEnumerable<string> topicExpressions);
    }
}
