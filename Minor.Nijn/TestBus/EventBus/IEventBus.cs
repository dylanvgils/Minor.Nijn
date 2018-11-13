using System.Collections.Generic;

namespace Minor.Nijn.TestBus.EventBus
{
    internal interface IEventBus : IBus<EventMessage>
    {
        EventBusQueue DeclareQueue(string queueName, IEnumerable<string> topicExpressions);
    }
}
