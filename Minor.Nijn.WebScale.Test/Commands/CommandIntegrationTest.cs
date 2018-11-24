using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Minor.Nijn.WebScale.Test.TestClasses;
using Minor.Nijn.WebScale.Test.TestClasses.Commands;
using Minor.Nijn.WebScale.Test.TestClasses.Domain;
using Minor.Nijn.WebScale.Test.TestClasses.Injectable;
using Newtonsoft.Json;

namespace Minor.Nijn.WebScale.Commands.Test
{
    [TestClass]
    public class CommandIntegrationTest
    {
        [TestCleanup]
        public void AfterEach()
        {
            OrderCommandListener.HandleOrderCreatedEventHasBeenCalled = false;
            OrderCommandListener.HandleOrderCreatedEventHasBeenCalledWith = null;
        }

        [TestMethod]
        public void RegisteredDependenciesCanBeInjected()
        {
            var foo = new Foo();

            var busContext = new TestBusContextBuilder().CreateTestContext();
            var hostBuilder = new MicroserviceHostBuilder()
                .RegisterDependencies(services =>
                {
                    services.AddSingleton<IFoo>(foo);
                })
                .WithContext(busContext)
                .AddEventListener<ProductCommandListener>();

            using (var host = hostBuilder.CreateHost())
            {
                host.RegisterListeners();
                Assert.IsTrue(foo.HasBeenInstantiated);
            }
        }

        [TestMethod]
        public void CommandListenerCanReceiveDomainCommands()
        {
            var correlationId = Guid.NewGuid().ToString();
            var commandQueue = TestClassesConstants.OrderCommandListenerQueueName;
            var order = new Order { Id = 1, Description = "Some description" };
            var addOrderCommand = new AddOrderCommand(commandQueue, order);
            var commandMessage = new CommandMessage(JsonConvert.SerializeObject(addOrderCommand), "type", correlationId, commandQueue);

            var busContext = new TestBusContextBuilder().CreateTestContext();
            var hostBuilder = new MicroserviceHostBuilder()
                .WithContext(busContext)
                .AddEventListener<OrderCommandListener>();

            using (var host = hostBuilder.CreateHost())
            {
                host.RegisterListeners();
                busContext.CommandBus.DispatchMessage(commandMessage);

                Assert.IsTrue(host.ListenersRegistered);
                Assert.IsTrue(OrderCommandListener.HandleOrderCreatedEventHasBeenCalled);

                var result = OrderCommandListener.HandleOrderCreatedEventHasBeenCalledWith;
                Assert.AreEqual(order.Id, result.Order.Id);
                Assert.AreEqual(order.Description, result.Order.Description);
            }
        }

        [TestMethod]
        [Ignore]
        public async Task CommandPublisherCanSendCommandMessage()
        {
            var commandQueue = TestClassesConstants.OrderCommandListenerQueueName;
            var order = new Order { Id = 1, Description = "Some description" };
            var addOrderCommand = new AddOrderCommand(commandQueue, order);

            var busContext = new TestBusContextBuilder().CreateTestContext();
            var hostBuilder = new MicroserviceHostBuilder()
                .WithContext(busContext)
                .AddEventListener<OrderCommandListener>();

            using (var host = hostBuilder.CreateHost())
            using (var publisher = new CommandPublisher(busContext))
            {
                host.RegisterListeners();

                var result = await publisher.Publish<Order>(addOrderCommand);

                Assert.IsTrue(host.ListenersRegistered);
                Assert.IsTrue(OrderCommandListener.HandleOrderCreatedEventHasBeenCalled);
            }
        }
    }
}
