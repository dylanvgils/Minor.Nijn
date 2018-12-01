﻿using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Test.TestClasses.Commands;
using System;
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
        public int HandleAddProductCommand(AddProductCommand command)
        {
            return 42;
        }
    }
}
