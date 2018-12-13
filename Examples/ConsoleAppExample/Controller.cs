using ConsoleAppExample.Domain;
using Microsoft.Extensions.Logging;
using Minor.Nijn.WebScale.Commands;
using System.Threading.Tasks;

namespace ConsoleAppExample
{
    public class Controller
    {
        private readonly ILogger _logger;
        private readonly ICommandPublisher _publisher;

        public Controller(ICommandPublisher publisher)
        {
            _publisher = publisher;

            _logger = ConsoleAppExampleLogger.CreateLogger<Controller>();
        }

        public async Task SayHello(string name)
        {
            var request = new SayHelloCommand(name, "ConsoleAppExampleCommandQueue");
            var response = await _publisher.Publish<string>(request);
            _logger.LogInformation("Received command response with message: {0}", response);
        }

        public async Task WhoopsExceptionThrown()
        {
            var request = new SayHelloCommand("ThrowException", "ConsoleAppExampleExceptionCommandQueue");

            try
            {
                await _publisher.Publish<string>(request);
            }
            catch (CustomException e)
            {
                _logger.LogInformation("Custom exception with message was thrown: {0}", e.Message);
            }
        }

        public async Task AsyncCommandResponse()
        {
            var request = new SayHelloCommand("SomeName", "ConsoleAppExampleAsyncCommandQueue");

            var task = _publisher.Publish<string>(request);
            _logger.LogInformation("Hi i'm executed before the async command is completed!");

            await task;
        }
    }
}
