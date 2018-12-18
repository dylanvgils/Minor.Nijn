using System.Threading.Tasks;
using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Test.TestClasses.Commands;

namespace Minor.Nijn.WebScale.Test.InvalidTestClasses
{
    [CommandListener]
    public class InvalidCommandListenerAsyncReturnType
    {
        [Command("InvalidAsyncReturnTypeQueue")]
        public async Task InvalidAsyncReturnType(AddProductCommand request)
        {

        }
    }
}