using Minor.Nijn.WebScale.Attributes;

namespace Minor.Nijn.WebScale.Test.InvalidTestClasses
{
    [CommandListener]
    public class InvalidCommandListenerEventMessageParamType
    {
        [Command("QueueName")]
        public long ParameterTypeEventMessageInvalid(EventMessage message)
        {
            return 42;
        }
    }
}