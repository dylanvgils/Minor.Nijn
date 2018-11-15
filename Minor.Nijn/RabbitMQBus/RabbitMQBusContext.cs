using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System;
using System.Collections.Generic;


namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQBusContext : IRabbitMQBusContext
    {
        public IConnection Connection { get; private set; }
        public string ExchangeName { get; private set; }

        public  RabbitMQBusContext(IConnection connection, string exchangeName)
        {
            Connection = connection;
            ExchangeName = exchangeName;

            ILoggerFactory loggerFactory = new LoggerFactory();

            loggerFactory.AddProvider(
              new ConsoleLoggerProvider(
                (text, logLevel) => logLevel >= LogLevel.Debug, true));

            loggerFactory
              .AddSerilog(new LoggerConfiguration()
                      .MinimumLevel.Verbose()
                      .Enrich.FromLogContext()
                      .WriteTo.Console()
                      .WriteTo.File(new JsonFormatter(), "log.json", LogEventLevel.Warning)
                      .CreateLogger());

            NijnLogging.LoggerFactory = loggerFactory;
        }

        public IMessageSender CreateMessageSender()
        {
            return new RabbitMQMessageSender(this);
        }

        public IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions)
        {
            return new RabbitMQMessageReceiver(this, queueName, topicExpressions);
        }
        
        public ICommandSender CreateCommandSender()
        {
            return new RabbitMQCommandSender(this);
        }

        public ICommandReplySender CreateCommandReplySender(string replyTo)
        {
            return new RabbitMQCommandReplySender(this, replyTo);
        }

        public ICommandReceiver CreateCommandReceiver()
        {
            return new RabbitMQCommandReceiver(this, "commandQueueName");
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}
