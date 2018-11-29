using Minor.Nijn.WebScale.Attributes;

namespace Minor.Nijn.WebScale.Test.InvalidTestClasses
{
    [CommandListener]
    public class InvalidCommandParameterType
    {
        [Command("QueueName")]
        public void ParameterTypeInvalid(string order)
        {
        }
    }
}