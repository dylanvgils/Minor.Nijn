using System.Diagnostics;
using System.Threading.Tasks;
using ConsoleAppExample.DAL;
using ConsoleAppExample.Domain;
using ConsoleAppExample.Listeners;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ConsoleAppExample.Test.Listeners
{
    [TestClass]
    public class EventListenerTest
    {
        private Mock<IDataMapper<string>> _dataMapperMock;
        private EventListener _target;

        [TestInitialize]
        public void BeforeEach()
        {
            _dataMapperMock = new Mock<IDataMapper<string>>(MockBehavior.Strict);
            _target = new EventListener(_dataMapperMock.Object);
        }

        [TestMethod]
        public void HandleSaidHelloEvent_ShouldHandleEvent()
        {
            var evt = new SaidHelloEvent("Hello, Testje", "RoutingKey");
            _dataMapperMock.Setup(d => d.Save(evt.Message));

            _target.HandleSaidHelloEvent(evt);

            _dataMapperMock.VerifyAll();
        }

        [TestMethod]
        public async Task HandleSaidHelloEventAsync_ShouldHandleEvent()
        {
            var evt = new SaidHelloEvent("Hello, Testje", "RoutingKey");
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            await _target.HandleSaidHelloEventAsync(evt);
            stopwatch.Stop();

            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= 1000, "Elapsed time should be at least 1 second");
        }
    }
}