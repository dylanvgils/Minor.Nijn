using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Test.TestClasses.Commands;
using System;
using System.Threading.Tasks;
using Minor.Nijn.WebScale.Test.TestClasses.Injectable;

namespace Minor.Nijn.WebScale.Test.TestClasses
{
    [CommandListener]
    public class ProductCommandListener
    {
        public ProductCommandListener(IFoo foo)
        {
            foo.HasBeenInstantiated = true;
        }

        [Command(TestClassesConstants.ProductCommandListenerQueueName)]
        public async Task<int> HandleAddProductCommand(AddProductCommand command)
        {
            var task = Task.Run<int>(() => 42);
            return await task;
        }
    }
}
