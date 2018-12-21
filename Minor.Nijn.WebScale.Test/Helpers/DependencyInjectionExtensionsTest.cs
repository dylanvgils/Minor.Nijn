using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Events;
using Minor.Nijn.WebScale.Helpers;

namespace Minor.Nijn.WebScale.Test.Helpers
{
    [TestClass]
    public class DependencyInjectionExtensionsTest
    {
        [TestMethod]
        public void AddNijnWebScale_ShouldReturnIServiceCollectionWithDependencies()
        {
            var services = new ServiceCollection();

            services.AddNijnWebScale();

            Assert.AreEqual(2, services.Count);
            Assert.IsTrue(
                services.Any(s => s.ServiceType == typeof(ICommandPublisher) && s.ImplementationType == typeof(CommandPublisher))
                , "Should contain command publisher"
            );
            Assert.IsTrue(
                services.Any(s => s.ServiceType == typeof(IEventPublisher) && s.ImplementationType == typeof(EventPublisher))
                , "should contain event publisher"
            );
        }

        [TestMethod]
        public void AddNijnWebScale_ShouldReturnMicroserviceHostBuilderWhenCalledWithAction()
        {
            var services = new ServiceCollection();

            var result = services.AddNijnWebScale(options => { });

            Assert.IsInstanceOfType(result, typeof(MicroserviceHostBuilder));
            Assert.AreEqual(services, result.ServiceCollection);
            Assert.AreEqual(2, result.ServiceCollection.Count);
        }
    }
}