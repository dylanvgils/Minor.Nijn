using System;
using System.Threading.Tasks;

namespace Minor.Nijn
{
    /// <summary>
    /// Sender for commands
    /// </summary>
    public interface ICommandSender : IDisposable
    {
        /// <summary>
        /// Sends command message
        /// </summary>
        /// <param name="request">command to send</param>
        /// <returns>received result</returns>
        Task<ResponseCommandMessage> SendCommandAsync(RequestCommandMessage request);
    }
}
