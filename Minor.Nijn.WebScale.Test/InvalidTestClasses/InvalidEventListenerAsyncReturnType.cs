using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Test.TestClasses.Events;

namespace Minor.Nijn.WebScale.Test.InvalidTestClasses
{
    [EventListener("QueueName")]
    public class InvalidEventListenerAsyncReturnType
    {
        [Topic("a.b.c")]
        public async void InvalidAsyncReturnType(OrderCreatedEvent evt)
        {
        }
    }
}