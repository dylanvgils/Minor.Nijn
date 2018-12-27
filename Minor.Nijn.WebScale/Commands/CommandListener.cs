using Microsoft.Extensions.Logging;
using Minor.Nijn.WebScale.Helpers;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace Minor.Nijn.WebScale.Commands
{
    internal class CommandListener : ICommandListener
    {
        private readonly ILogger _logger;

        public CommandListenerInfo Meta { get; }
        public string QueueName => Meta.QueueName;

        private ICommandReceiver _receiver;
        private IMicroserviceHost _host;
        private bool _isListening;
        private bool _disposed;

        public CommandListener(CommandListenerInfo meta)
        {
            Meta = meta;
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

            _host = host;
            _receiver = host.Context.CreateCommandReceiver(QueueName);
            _receiver.DeclareCommandQueue();
            _receiver.StartReceivingCommands(HandleCommandMessage);

            _isListening = true;
        }

        public ResponseCommandMessage HandleCommandMessage(RequestCommandMessage request)
        {
            var instance = _host.CreateInstance(Meta.Type);

            ResponseCommandMessage response;

            try
            {
                CheckInputType(request);

                var payload = CreatePayload(request);
                var json = InvokeListener(instance, payload);

                response = new ResponseCommandMessage(json, Meta.Method.ReturnType.Name, request.CorrelationId);
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
            if (request.Type == Meta.CommandType.Name) return;

            _logger.LogError(
                "Received command in invalid format, expected message to be of type {0} and got {1}",
                Meta.CommandType.Name, request.Type);

            throw new ArgumentException(
                $"Received command with wrong type, type was {request.Type} and expected {Meta.CommandType.Name}");
        }

        private object CreatePayload(RequestCommandMessage request)
        {
            var payload = JsonConvert.DeserializeObject(request.Message, Meta.CommandType);

            // TODO: Set these properties through the JSON Deserializer
            payload.GetType().GetProperty("CorrelationId").SetValue(payload, request.CorrelationId);
            payload.GetType().GetProperty("Timestamp").SetValue(payload, request.Timestamp);

            return payload;
        }

        private string InvokeListener(object instance, params object[] payload)
        {
            var result = Meta.IsAsyncMethod 
                ? Meta.Method.InvokeAsync(instance, payload).Result 
                : Meta.Method.Invoke(instance, payload);

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