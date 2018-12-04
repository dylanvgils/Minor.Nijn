using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Test.TestClasses.Commands;

namespace Minor.Nijn.WebScale.Test.InvalidTestClasses
{
    [CommandListener]
    public class InvalidCommandListenerReturnType
    {
        [Command("InvalidReturnTypeQueue")]
        public void InvalidReturnType(AddProductCommand request)
        {

        }
    }
}