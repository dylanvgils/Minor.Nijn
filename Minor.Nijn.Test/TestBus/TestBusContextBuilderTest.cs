using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;

namespace Minor.Nijn.Test.TestBus
{
    [TestClass]
    public class TestBusContextBuilderTest
    {
        [TestMethod]
        public void CreateContext_ShouldReturnNewTestBusContextInstance()
        {
            var target = TestBusContextBuilder.CreateContext();
            Assert.IsInstanceOfType(target, typeof(TestBusContext));
        }

        [TestMethod]
        public void CreateContext_ShouldReturnSameInstanceWhenCalledTheSecondTime()
        {
            var context1 = TestBusContextBuilder.CreateContext();
            var context2 = TestBusContextBuilder.CreateContext();

            Assert.AreEqual(context1, context2);
        }
    }
}
