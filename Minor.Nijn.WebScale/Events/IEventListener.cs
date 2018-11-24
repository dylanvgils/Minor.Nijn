using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Minor.Nijn.WebScale.Events
{
    public interface IEventListener : IDisposable
    {
        string QueueName { get; }
        IEnumerable<string> TopicExpressions { get; }

        void StartListening(IMicroserviceHost host);
    }
}