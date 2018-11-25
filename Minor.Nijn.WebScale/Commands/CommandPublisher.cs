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

        public async Task<T> Publish<T>(DomainCommand domainCommand)
        {
            var body = JsonConvert.SerializeObject(domainCommand);
            var command = new RequestCommandMessage(body, typeof(T).Name, domainCommand.CorrelationId, domainCommand.RoutingKey);
            var result = await _sender.SendCommandAsync(command);
            return JsonConvert.DeserializeObject<T>(result.Message);
        }

        public void Dispose()
        {
            _sender?.Dispose();
        }
    }
}