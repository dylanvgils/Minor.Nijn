using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Helpers;
using Moq;
using RabbitMQ.Client;

namespace Minor.Nijn.Test.Helpers
{
    [TestClass]
    public class DependencyInjectionExtensionsTest
    {
        [TestMethod]
        public void AddNijn_ShouldReturnServiceCollectionWithDependencies()
        {
            var mock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            var services = new ServiceCollection();

            services.AddNijn(mock.Object);

            Assert.AreEqual(1, services.Count);
            Assert.IsTrue(services.Any(s => s.ServiceType == typeof(IBusContext<IConnection>)));
        }

        [TestMethod]
        public void AddNijn_ShouldThrowExceptionWhenContextIsNull()
        {
            var services = new ServiceCollection();

            Action action = () => { services.AddNijn((IBusContext<IConnection>) null); };

            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual("Context can not be null", ex.Message);
        }
    }
}