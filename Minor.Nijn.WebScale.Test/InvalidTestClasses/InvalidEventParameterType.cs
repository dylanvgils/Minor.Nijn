using Minor.Nijn.WebScale.Attributes;

namespace Minor.Nijn.WebScale.Test.InvalidTestClasses
{
    [EventListener("QueueName")]
    public class InvalidEventParameterType
    {
        [Topic("a.b.c")]
        public void ParameterTypeInvalid(string order)
        {
        }
    }
}