using System;

namespace Minor.Nijn.WebScale.Events
{
    public interface IEventPublisher : IDisposable
    {
        void Publish(DomainEvent domainEvent);
    }
}
