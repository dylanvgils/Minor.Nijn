using System.Collections.Generic;

namespace Minor.Nijn.TestBus.EventBus
{
    public interface IEventBus : ITestBus<EventMessage>
    {
        EventBusQueue DeclareQueue(string queueName, IEnumerable<string> topicExpressions);
    }
}
