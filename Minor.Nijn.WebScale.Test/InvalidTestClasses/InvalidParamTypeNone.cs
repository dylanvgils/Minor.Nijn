using Minor.Nijn.WebScale.Attributes;

namespace Minor.Nijn.WebScale.Test.InvalidTestClasses
{
    [EventListener("QueueName")]
    public class InvalidParamTypeNone
    {
        [Topic("topic")]
        public void ParamTypeInvalidNone()
        {

        }
    }
}