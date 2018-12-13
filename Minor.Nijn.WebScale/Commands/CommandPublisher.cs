using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Minor.Nijn.WebScale.Commands
{
    public class CommandPublisher : ICommandPublisher
    {
        private readonly ILogger logger;

        private readonly Assembly _callingAssembly;
        private readonly ICommandSender _sender;
        private bool _disposed;

        public CommandPublisher(IBusContext<IConnection> context)
        {
            _callingAssembly = Assembly.GetCallingAssembly();
            _sender = context.CreateCommandSender();

            logger = NijnWebScaleLogger.CreateLogger<CommandPublisher>();
        }

        public async Task<T> Publish<T>(DomainCommand domainCommand)
        {
            CheckDisposed();

            var body = JsonConvert.SerializeObject(domainCommand);

            var command = new RequestCommandMessage(
                message: body, 
                type: domainCommand.GetType().Name, 
                correlationId: domainCommand.CorrelationId, 
                routingKey: domainCommand.RoutingKey,
                timestamp: domainCommand.Timestamp
            );

            var result = await _sender.SendCommandAsync(command);

            if (result.Type.Contains("Exception"))
            {
                ThrowException(result);
            }

            return JsonConvert.DeserializeObject<T>(result.Message);
        }

        private void ThrowException(CommandMessage result)
        {
            object exception;

            try
            {
                var jObject = JObject.Parse(result.Message);
                var className = (string) jObject.GetValue("ClassName");

                var type =
                    _callingAssembly.GetType(className)
                    ?? Assembly.GetEntryAssembly().GetType(className)
                    ?? Type.GetType(className) 
                    ?? typeof(Exception);

                exception = jObject.ToObject(type);
            }
            catch (Exception)
            {
                logger.LogWarning("Unknown exception occured of type {0}", result.Type);
                throw new InvalidCastException($"Unknown exception occurred of type '{result.Type}' with message: {result.Message}");
            }

            throw exception as Exception;
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

        ~CommandPublisher()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _sender?.Dispose();
            }

            _disposed = true;
        }
    }
}