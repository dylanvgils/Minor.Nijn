using System;

namespace Minor.Nijn.WebScale.Events
{
    /// <summary>
    /// Publisher for sending domain events
    /// </summary>
    public interface IEventPublisher : IDisposable
    {
        /// <summary>
        /// Publishes a event to the specified topic
        /// </summary>
        /// <param name="domainEvent">Event tot send</param>
        void Publish(DomainEvent domainEvent);
    }
}
