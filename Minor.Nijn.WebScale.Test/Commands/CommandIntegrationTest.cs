using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Minor.Nijn.TestBus.CommandBus;
using Minor.Nijn.WebScale.Test.TestClasses;
using Minor.Nijn.WebScale.Test.TestClasses.Commands;
using Minor.Nijn.WebScale.Test.TestClasses.Injectable;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Order = Minor.Nijn.WebScale.Test.TestClasses.Domain.Order;

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

            var commandMessage = new RequestCommandMessage("Message", "type", "correlationId", TestClassesConstants.ProductCommandListenerQueueName);
            var request = new TestBusCommand("", commandMessage);

            var busContext = new TestBusContextBuilder().CreateTestContext();
            var hostBuilder = new MicroserviceHostBuilder()
                .RegisterDependencies(services =>
                {
                    services.AddSingleton<IFoo>(foo);
                })
                .WithContext(busContext)
                .AddListener<ProductCommandListener>();

            using (var host = hostBuilder.CreateHost())
            {
                host.RegisterListeners();
                busContext.CommandBus.DispatchMessage(request);

                Assert.IsTrue(foo.HasBeenInstantiated);
            }
        }

        [TestMethod]
        public void CommandListenerCanReceiveDomainCommands()
        {
            var commandQueue = TestClassesConstants.OrderCommandListenerQueueName;
            var order = new Order { Id = 1, Description = "Some description" };
            var addOrderCommand = new AddOrderCommand(commandQueue, order);

            var commandMessage = new RequestCommandMessage(
                message: JsonConvert.SerializeObject(addOrderCommand), 
                type: addOrderCommand.GetType().Name, 
                correlationId: addOrderCommand.CorrelationId, 
                routingKey:commandQueue,
                timestamp: addOrderCommand.Timestamp
            );

            var command = new TestBusCommand(null, commandMessage);

            var busContext = new TestBusContextBuilder().CreateTestContext();
            var hostBuilder = new MicroserviceHostBuilder()
                .WithContext(busContext)
                .AddListener<OrderCommandListener>();

            using (var host = hostBuilder.CreateHost())
            {
                host.RegisterListeners();
                busContext.CommandBus.DispatchMessage(command);

                Assert.IsTrue(host.ListenersRegistered, "Listeners are registered");
                Assert.IsTrue(OrderCommandListener.HandleOrderCreatedEventHasBeenCalled, "Event listener has been called");

                var result = OrderCommandListener.HandleOrderCreatedEventHasBeenCalledWith;
                Assert.AreEqual(commandMessage.RoutingKey, result.RoutingKey);
                Assert.AreEqual(commandMessage.CorrelationId, result.CorrelationId);
                Assert.AreEqual(commandMessage.Timestamp, result.Timestamp);
                Assert.AreEqual(order.Id, result.Order.Id);
                Assert.AreEqual(order.Description, result.Order.Description);
            }
        }

        [TestMethod]
        public async Task CommandPublisherCanSendCommandMessage()
        {
            var replyOrder = new Order {Id = 100, Description = "Some reply description"};
            OrderCommandListener.ReplyWith = replyOrder;

            var commandQueue = TestClassesConstants.OrderCommandListenerQueueName;
            var order = new Order { Id = 1, Description = "Some description" };
            var addOrderCommand = new AddOrderCommand(commandQueue, order);

            var busContext = new TestBusContextBuilder().CreateTestContext();
            var hostBuilder = new MicroserviceHostBuilder()
                .WithContext(busContext)
                .AddListener<OrderCommandListener>();

            using (var host = hostBuilder.CreateHost())
            using (var publisher = new CommandPublisher(busContext))
            {
                host.RegisterListeners();
 
                var result = await publisher.Publish<Order>(addOrderCommand);
                
                Assert.IsTrue(host.ListenersRegistered, "Listeners are registered");
                Assert.IsTrue(OrderCommandListener.HandleOrderCreatedEventHasBeenCalled, "Command listener has been called");
                Assert.AreEqual(replyOrder.Id, result.Id);
                Assert.AreEqual(replyOrder.Description, result.Description);
            }
        }
    }
}
