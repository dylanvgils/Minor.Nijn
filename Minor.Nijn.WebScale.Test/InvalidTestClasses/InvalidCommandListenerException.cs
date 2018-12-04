using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Test.TestClasses.Commands;
using System;

namespace Minor.Nijn.WebScale.Test.InvalidTestClasses
{
    [CommandListener]
    class InvalidCommandListenerException
    {
        [Command("InvalidCommandQueue")]
        public int ThrowException(AddProductCommand request)
        {
            throw new NullReferenceException("Some null reference exception");
        }
    }
}
