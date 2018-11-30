﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace Minor.Nijn.WebScale.Commands
{
    public class CommandListener : ICommandListener
    {
        private readonly ILogger _logger;

        public string QueueName { get; }

        private readonly Type _type;
        private readonly MethodInfo _method;
        private readonly Type _commandType;
        private object _instance;

        private ICommandReceiver _receiver;
        private bool _isListening;
        private bool _disposed;

        public CommandListener(Type type, MethodInfo method, string queueName)
        {
            QueueName = queueName;
            _type = type;

            _method = method;
            _commandType = method.GetParameters()[0].ParameterType;

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
            if (request.Type != _commandType.Name)
            {
                _logger.LogError("Received command in invalid format, expected message to be of type {0} and got {1}",
                    _commandType.Name, request.Type);
                return null; // TODO: Throw exception as response
            }

            var payload = JsonConvert.DeserializeObject(request.Message, _commandType);

            // TODO: Set these properties through the JSON Deserializer
            payload.GetType().GetProperty("CorrelationId").SetValue(payload, request.CorrelationId);
            payload.GetType().GetProperty("Timestamp").SetValue(payload, request.Timestamp);

            var result = _method.Invoke(_instance, new [] { payload });
            var json = JsonConvert.SerializeObject(result);

            return new ResponseCommandMessage(json, _method.ReturnType.Name, request.CorrelationId);
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