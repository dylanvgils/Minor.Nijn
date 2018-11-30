using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Minor.Nijn.WebScale.Commands
{
    public class CommandPublisher : ICommandPublisher
    {
        private readonly ICommandSender _sender;
        private bool _disposed;

        public CommandPublisher(IBusContext<IConnection> context)
        {
            _sender = context.CreateCommandSender();
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

        private static void ThrowException(CommandMessage result)
        {
            object exception;

            try
            {
                var jsonObject = JObject.Parse(result.Message);
                var type = Type.GetType(jsonObject["ClassName"].ToString());
                exception = jsonObject.ToObject(type);

                if (!exception.GetType().IsSubclassOf(typeof(Exception)))
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                throw new Exception($"Unknown exception occurred of type '{result.Type}' with message: {result.Message}");
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