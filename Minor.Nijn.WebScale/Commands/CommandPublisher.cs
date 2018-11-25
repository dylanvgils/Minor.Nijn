using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Threading.Tasks;

namespace Minor.Nijn.WebScale.Commands
{
    public class CommandPublisher : ICommandPublisher
    {
        private readonly ICommandSender _sender;

        public CommandPublisher(IBusContext<IConnection> context)
        {
            _sender = context.CreateCommandSender();
        }

        public Task<T> Publish<T>(DomainCommand domainCommand)
        {
            return Task.Run(async () =>
            {
                var body = JsonConvert.SerializeObject(domainCommand);
                var command = new CommandMessage(body, typeof(T).Name, domainCommand.CorrelationId, domainCommand.RoutingKey);
                var result = await _sender.SendCommandAsync(command);
                return JsonConvert.DeserializeObject<T>(result.Message);
            });
        }

        public void Dispose()
        {
            _sender?.Dispose();
        }
    }
}