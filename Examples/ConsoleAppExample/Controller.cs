using System;
using ConsoleAppExample.Domain;
using Microsoft.Extensions.Logging;
using Minor.Nijn.WebScale.Commands;
using System.Threading.Tasks;
using ClassLibraryExample;
using Minor.Nijn.WebScale.Events;

namespace ConsoleAppExample
{
    public class Controller
    {
        private readonly ILogger _logger;
        private readonly ICommandPublisher _commandPublisher;
        private readonly IEventPublisher _eventPublisher;

        public Controller(ICommandPublisher commandPublisher, IEventPublisher eventPublisher)
        {
            _commandPublisher = commandPublisher;
            _eventPublisher = eventPublisher;

            _logger = ConsoleAppExampleLogger.CreateLogger<Controller>();
        }

        public async Task<string> SayHello(string name)
        {
            var request = new SayHelloCommand(name, "ConsoleAppExampleCommandQueue");
            var response = await _commandPublisher.Publish<string>(request);
            _logger.LogInformation("Received command response with message: {0}", response);
            return response;
        }

        public async Task<string> WhoopsExceptionThrown()
        {
            var request = new SayHelloCommand("ThrowException", "ConsoleAppExampleExceptionCommandQueue");

            try
            {
                await _commandPublisher.Publish<string>(request);
                return "No exception thrown";
            }
            catch (CustomException e)
            {
                _logger.LogInformation("Custom exception with message was thrown: {0}", e.Message);
                return "Exception thrown";
            }
        }

        public async Task<string> WhoopsExternalExceptionThrown()
        {
            var request = new SayHelloCommand("ThrowException", "ConsoleAppExampleExternalExceptionCommandQueue");

            try
            {
                await _commandPublisher.Publish<string>(request);
                return "No exception thrown";
            }
            catch (ExternalCustomException e)
            {
                _logger.LogInformation("External custom exception with message was thrown: {0}", e.Message);
                return "Exception thrown";
            }
        }

        public async Task AsyncCommandListenerMethod()
        {
            var request = new SayHelloCommand("SomeName", "ConsoleAppExampleAsyncCommandQueue");

            var task = _commandPublisher.Publish<string>(request);
            _logger.LogInformation("Hi i'm executed before the async command is completed!");

            await task;
        }

        public void AsyncEventListenerMethod()
        {
            var request = new SaidHelloEvent("Hello hello", "SaidHelloAsync");
            _eventPublisher.Publish(request);
        }
    }
}
