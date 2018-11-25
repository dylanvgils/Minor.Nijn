using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Minor.Nijn.WebScale.Commands
{
    public class CommandListener : ICommandListener
    {
        private readonly ILogger _logger;

        public string QueueName { get; }

        private readonly Type _type;
        private readonly MethodInfo _method;
        private object _instance;

        private ICommandReceiver _receiver;
        private bool _isListening;

        public CommandListener(Type type, MethodInfo method, string queueName)
        {
            QueueName = queueName;
            _type = type;
            _method = method;

            _logger = NijnWebScaleLogger.CreateLogger<CommandListener>();
        }
        public void StartListening(IMicroserviceHost host)
        {
            if (_isListening)
            {
                _logger.LogDebug("Event listener already listening for commands");
                throw new InvalidOperationException("Already listening for commands");
            }

            _instance = host.CreateInstance(_type);

            _receiver = host.Context.CreateCommandReceiver(QueueName);
            _receiver.DeclareCommandQueue();
            _receiver.StartReceivingCommands(HandleCommandMessage);

            _isListening = true;
        }

        // TODO: Add parameter check, parameter has to be derived type of DomainCommand
        private ResponseCommandMessage HandleCommandMessage(RequestCommandMessage message)
        {
            var paramType = _method.GetParameters()[0].ParameterType;
            var payload = JsonConvert.DeserializeObject(message.Message, paramType);

            var result = _method.Invoke(_instance, new [] { payload });
            var json = JsonConvert.SerializeObject(result);

            return new ResponseCommandMessage(json, _method.ReturnType.Name, message.CorrelationId);
        }

        public void Dispose()
        {
            _receiver?.Dispose();
        }
    }
}