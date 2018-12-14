using System.Collections.Generic;
using ConsoleAppExample.Domain;
using ConsoleAppExample.Listeners;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Minor.Nijn.WebScale.Events;

namespace ConsoleAppExample.Test.Integration
{
    [TestClass]
    public class CommandListenerIntegrationTest
    {
        private ITestBusContext _context;
        private CommandListener _target;

        [TestInitialize]
        public void BeforeEach()
        {
            _context = new TestBusContextBuilder().CreateTestContext();
            var eventPublisher = new EventPublisher(_context);

            _target = new CommandListener(eventPublisher);
        }

        [TestMethod]
        public void ShouldHandleSayHelloCommand()
        {
            var eventQueueName = "EventQueueName";
            var name = "Testje";
            var responseMessage = $"Hello, {name}";
            var requestCommand = new SayHelloCommand(name, "RoutingKey");

            _context.EventBus.DeclareQueue(eventQueueName, new List<string> { "SaidHello" });

            var result = _target.HandleSayHelloCommand(requestCommand);

            Assert.AreEqual(responseMessage, result);
            Assert.AreEqual(1, _context.EventBus.Queues[eventQueueName].MessageQueueLength);
        }
    }
}