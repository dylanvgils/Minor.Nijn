using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Test.TestClasses.Events;

namespace Minor.Nijn.WebScale.Test.InvalidTestClasses
{
    [EventListener("QueueName")]
    public class InvalidEventParametersLength
    {
        [Topic("a.b.c")]
        public void ToManyParameters(OrderCreatedEvent evt, string param2)
        {
        }
    }
}