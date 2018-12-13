using ConsoleAppExample.DAL;
using ConsoleAppExample.Domain;
using Microsoft.Extensions.Logging;
using Minor.Nijn.WebScale.Attributes;

namespace ConsoleAppExample.Listeners
{
    [EventListener("ConsoleAppExample.SaidHelloEventListenerQueue")]
    public class EventListener
    {
        private readonly ILogger _logger;
        private readonly IDataMapper<string, long> _dataMapper;

        public EventListener(IDataMapper<string, long> dataMapper)
        {
            _dataMapper = dataMapper;
            _logger = ConsoleAppExampleLogger.CreateLogger<EventListener>();
        }

        [Topic("ConsoleAppExample.SaidHello")]
        public void HandleSaidHelloEvent(SaidHelloEvent evt)
        {
            _dataMapper.Save(evt.Message);
            _logger.LogInformation("Received event with message: {0}", evt.Message);
        }
    }
}
