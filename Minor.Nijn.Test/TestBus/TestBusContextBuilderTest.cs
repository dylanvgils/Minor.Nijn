using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;

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
        public void WithMockConnection_ShouldSetConnectionProperty()
        {
            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            var target = new TestBusContextBuilder().WithMockConnection(connectionMock.Object);
            Assert.AreEqual(connectionMock.Object, target.Connection);
        }

        [TestMethod]
        public void WithMockConnection_ShouldSetConnectionTimeout()
        {
            var target = new TestBusContextBuilder().WithConnectionTimeout(500);
            Assert.AreEqual(500, target.ConnectionTimeoutAfterMs);
        }
    }
}
