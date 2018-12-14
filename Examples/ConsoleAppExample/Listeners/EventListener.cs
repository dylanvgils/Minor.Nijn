using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ConsoleAppExample.DAL;
using ConsoleAppExample.Domain;
using Microsoft.Extensions.Logging;
using Minor.Nijn.WebScale.Attributes;

namespace ConsoleAppExample.Listeners
{
    [EventListener("ConsoleAppExampleEventQueue")]
    public class EventListener
    {
        private readonly ILogger _logger;
        private readonly IDataMapper<string, long> _dataMapper;

        public EventListener(IDataMapper<string, long> dataMapper)
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

            _logger.LogInformation("Hi from async EventListener method, time elapsed is: {0} ms", stopwatch.ElapsedMilliseconds);
        }
    }
}
