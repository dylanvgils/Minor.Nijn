using ConsoleAppExample.Domain;
using Microsoft.Extensions.Logging;
using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Events;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ClassLibraryExample;

namespace ConsoleAppExample.Listeners
{
    [CommandListener]
    public class CommandListener
    {
        private readonly ILogger _logger;
        private readonly IEventPublisher _publisher;

        public CommandListener(IEventPublisher publisher)
        {
            _publisher = publisher;

            _logger = ConsoleAppExampleLogger.CreateLogger<CommandListener>();
        }

        [Command("ConsoleAppExampleCommandQueue")]
        public string HandleSayHelloCommand(SayHelloCommand request)
        {
            var response = "Hello, " + request.Name;

            var saidHelloEvent = new SaidHelloEvent(response, "SaidHello");
            _publisher.Publish(saidHelloEvent);

            return response;
        }

        [Command("ConsoleAppExampleExceptionCommandQueue")]
        public string HandleException(SayHelloCommand request)
        {
            throw new CustomException("Some custom exception message");
        }

        [Command("ConsoleAppExampleExternalExceptionCommandQueue")]
        public string HandleExternalException(SayHelloCommand request)
        {
            throw new ExternalCustomException("Some custom external exception message");
        }

        [Command("ConsoleAppExampleAsyncCommandQueue")]
        public async Task<string> HandleWithAsyncCommandResponse(SayHelloCommand request)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            await Task.Run(() => { Thread.Sleep(1000); });
            stopwatch.Stop();

            _logger.LogInformation("Hi from async CommandListener method, time elapsed is: {0} ms", stopwatch.ElapsedMilliseconds);
            return "Hello, " + request.Name;
        }
    }
}
