using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ConsoleAppExample.DAL;
using ConsoleAppExample.Domain;
using Microsoft.Extensions.Logging;
using Minor.Nijn;
using Minor.Nijn.WebScale.Attributes;

namespace ConsoleAppExample.Listeners
{
    [EventListener("ConsoleAppExampleEventQueue")]
    public class EventListener
    {
        private readonly ILogger _logger;
        private readonly IDataMapper<string> _dataMapper;

        public EventListener(IDataMapper<string> dataMapper)
        {
            _dataMapper = dataMapper;
            _logger = ConsoleAppExampleLogger.CreateLogger<EventListener>();
        }

        [Topic("SaidHello")]
        public void HandleSaidHelloEvent(SaidHelloEvent evt)
        {
            _dataMapper.Save(evt.Message);
            _logger.LogInformation("Received event with message: {0}", evt.Message);
        }

        [Topic("SaidHelloAsync")]
        public async Task HandleSaidHelloEventAsync(SaidHelloEvent evt)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            await Task.Run(() => { Thread.Sleep(1000); });
            stopwatch.Stop();

            _logger.LogInformation("Hi from async EventListener method, time elapsed is: {0} ms",
                stopwatch.ElapsedMilliseconds);
        }

        [Topic("SpamMeQueue", "AnotherQueueBinding")]
        public void HandleEventMessage(EventMessage message)
        {
            _logger.LogInformation("Received EventMessage with body: {0}", message.Message);
        }
    }
}
