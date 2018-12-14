using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn;
using Minor.Nijn.TestBus;
using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Events;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ConsoleAppExample.Test.Integration
{
    [TestClass]
    public class ControllerIntegrationTest
    {
        private ITestBusContext _context;

        private Controller _target;

        [TestInitialize]
        public void BeforeEach()
        {
            _context = new TestBusContextBuilder().CreateTestContext();

            var commandPublisher = new CommandPublisher(_context);
            var eventPublisher = new EventPublisher(_context);

            _target = new Controller(commandPublisher, eventPublisher);
        }

        [TestMethod]
        public async Task ShouldPublishSayHelloCommand()
        {
            var name = "Testje";
            var responseMessage = $"Hello, {name}";

            var receiver = _context.CreateCommandReceiver("ConsoleAppExampleCommandQueue");
            receiver.DeclareCommandQueue();
            receiver.StartReceivingCommands(request => new ResponseCommandMessage(
                message: JsonConvert.SerializeObject(responseMessage), 
                type: responseMessage.GetType().Name, 
                correlationId: request.CorrelationId
            ));

            var result = await _target.SayHello(name);

            Assert.AreEqual(responseMessage, result);
        }

        [TestMethod]
        public async Task ShouldHandleThrownException()
        {
            var exception = new CustomException("Some custom exception message");

            var receiver = _context.CreateCommandReceiver("ConsoleAppExampleExceptionCommandQueue");
            receiver.DeclareCommandQueue();
            receiver.StartReceivingCommands(request => throw exception);

            var result = await _target.WhoopsExceptionThrown();
            
            Assert.AreEqual("Exception thrown", result);
        }
    }
}