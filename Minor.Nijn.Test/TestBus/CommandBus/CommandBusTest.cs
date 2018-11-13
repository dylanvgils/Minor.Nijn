using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus.CommandBus;

namespace Minor.Nijn.Test.TestBus
{
    [TestClass]
    public class CommandBusTest
    {
        private CommandBus target;
        
        [TestInitialize]
        public void BeforeEach()
        {
            target = new CommandBus();
        }
        
        [TestMethod]
        public void DeclareCommandQueue_ShouldReturnNewCommandBusQueue()
        {
            string name = "RpcQueue";
           
            var result = target.DeclareCommandQueue("RpcQueue");
            
            Assert.IsInstanceOfType(result, typeof(CommandBusQueue));
            Assert.AreEqual(name, result.Name);
            Assert.AreEqual(1, target.QueueLength);
        }
    }
}