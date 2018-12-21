﻿using Microsoft.Extensions.Logging;

namespace Minor.Nijn.RabbitMQBus
{
    public interface IRabbitMQContextBuilder
    {
        RabbitMQContextBuilder WithExchange(string exchangeName);
        RabbitMQContextBuilder WithAddress(string hostName, int port);
        RabbitMQContextBuilder WithCredentials(string userName, string password);
        RabbitMQContextBuilder SetLoggerFactory(ILoggerFactory loggerFactory);
        RabbitMQContextBuilder ReadFromEnvironmentVariables();
        RabbitMQContextBuilder WithType(string type);
    }
}