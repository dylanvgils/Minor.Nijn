namespace Minor.Nijn.WebScale.Events
{
    public interface IEventPublisher
    {
        void Publish(DomainEvent domainEvent);
    }
}
