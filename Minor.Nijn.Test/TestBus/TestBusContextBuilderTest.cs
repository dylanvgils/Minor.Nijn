using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Minor.Nijn.TestBus.CommandBus;

namespace Minor.Nijn.TestBus.Test
{
    [TestClass]
    public class TestBusContextBuilderTest
    {
        [TestMethod]
        public void CreateTestContext_ShouldReturnTestBusContext()
        {
            var target = new TestBusContextBuilder().CreateTestContext();
            Assert.IsInstanceOfType(target, typeof(TestBusContext));
        }

        [TestMethod]
        public void WithCommandQueue_ShouldSetTheCommandQueueName()
        {
            IBusContextExtension target = new TestBusContextBuilder().WithCommandQueue("CommandQueue").CreateTestContext();

            Assert.AreEqual("CommandQueue", target.CommandQueueName);
        }
    }
}
