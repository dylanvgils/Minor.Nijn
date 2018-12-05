using System;
using System.Threading.Tasks;

namespace Minor.Nijn.WebScale.Commands
{
    /// <summary>
    /// Publisher for sending domain commands
    /// </summary>
    public interface ICommandPublisher : IDisposable
    {
        /// <summary>
        /// Publishes a command to the specified service and returns the result in the requested format
        /// </summary>
        /// <typeparam name="T">Return type of command</typeparam>
        /// <param name="domainCommand">Command tot send</param>
        /// <returns>The received response casted to the requested format</returns>
        Task<T> Publish<T>(DomainCommand domainCommand);
    }
}