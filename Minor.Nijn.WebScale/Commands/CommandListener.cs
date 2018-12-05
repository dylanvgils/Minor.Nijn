using Microsoft.Extensions.Logging;
using Minor.Nijn.WebScale.Helpers;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Minor.Nijn.WebScale.Commands
{
    internal class CommandListener : ICommandListener
    {
        private readonly ILogger _logger;

        public string QueueName { get; }

        private readonly Type _type;
        private readonly MethodInfo _method;
        private readonly bool _isAsyncMethod;
        private readonly Type _commandType;
        private object _instance;

        private ICommandReceiver _receiver;
        private bool _isListening;
        private bool _disposed;

        public CommandListener(Type type, MethodInfo method, string queueName)
        {
            QueueName = queueName;

            _type = type;
            _commandType = method.GetParameters()[0].ParameterType;

            _method = method;
            _isAsyncMethod = method.GetCustomAttribute<AsyncStateMachineAttribute>() != null;

            _logger = NijnWebScaleLogger.CreateLogger<CommandListener>();
        }
        public void StartListening(IMicroserviceHost host)
        {
            CheckDisposed();
            if (_isListening)
            {
                _logger.LogDebug("Command listener already listening for commands");
                throw new InvalidOperationException("Already listening for commands");
            }

            _instance = host.CreateInstance(_type);

            _receiver = host.Context.CreateCommandReceiver(QueueName);
            _receiver.DeclareCommandQueue();
            _receiver.StartReceivingCommands(HandleCommandMessage);

            _isListening = true;
        }

        internal ResponseCommandMessage HandleCommandMessage(RequestCommandMessage request)
        {
            ResponseCommandMessage response;

            try
            {
                CheckInputType(request);

                var payload = CreatePayload(request);
                var json = InvokeListener(payload);

                response = new ResponseCommandMessage(json, _method.ReturnType.Name, request.CorrelationId);
            }
            catch (TargetInvocationException e)
            {
                var json = JsonConvert.SerializeObject(e.InnerException);
                response = new ResponseCommandMessage(json, e.InnerException.GetType().Name, request.CorrelationId);
            }
            catch (Exception e)
            {
                var json = JsonConvert.SerializeObject(e);
                response = new ResponseCommandMessage(json, e.GetType().Name, request.CorrelationId);
            }

            return response;
        }

        private void CheckInputType(RequestCommandMessage request)
        {
            if (request.Type == _commandType.Name) return;

            _logger.LogError(
                "Received command in invalid format, expected message to be of type {0} and got {1}",
                _commandType.Name, request.Type);

            throw new ArgumentException(
                $"Received command with wrong type, type was {request.Type} and expected {_commandType.Name}");
        }

        private object CreatePayload(RequestCommandMessage request)
        {
            var payload = JsonConvert.DeserializeObject(request.Message, _commandType);

            // TODO: Set these properties through the JSON Deserializer
            payload.GetType().GetProperty("CorrelationId").SetValue(payload, request.CorrelationId);
            payload.GetType().GetProperty("Timestamp").SetValue(payload, request.Timestamp);

            return payload;
        }

        private string InvokeListener(params object[] payload)
        {
            var result = _isAsyncMethod 
                ? _method.InvokeAsync(_instance, payload).Result 
                : _method.Invoke(_instance, payload);

            return JsonConvert.SerializeObject(result);
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~CommandListener()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _receiver?.Dispose();
            }

            _disposed = true;
        }
    }
}