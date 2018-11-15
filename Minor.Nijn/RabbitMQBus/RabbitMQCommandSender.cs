using System.Threading.Tasks;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandSender : ICommandSender
    {
        public Task<CommandMessage> SendCommandAsync(CommandMessage request)
        {
            throw new System.NotImplementedException();
        }
        
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}