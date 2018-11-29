using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Test.TestClasses.Commands;

namespace Minor.Nijn.WebScale.Test.InvalidTestClasses
{
    [CommandListener]
    public class InvalidCommandParameterLength
    {
        [Command("CommandQueue")]
        public void ToManyParameters(AddOrderCommand command, string test)
        {
        }
    }
}