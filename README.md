# Minor.Nijn [![Build Status](https://travis-ci.org/dylanvgils/Minor.Nijn.svg?branch=master)](https://travis-ci.org/dylanvgils/Minor.Nijn) [![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=Minor.Nijn&metric=alert_status)](https://sonarcloud.io/dashboard?id=Minor.Nijn)

C# Wrapper for RabbitMQ client.

**Disclaimer:** *First off this is a framework built for educational purposes and is by no means intended for production use.*

The Minor.Nijn framework consists of two parts (Minor.Nijn and Minor.Nijn.WebScale). Minor.Nijn contains the logic for connecting to and handling the communication with RabbitMQ. Besides the real implementation, it also includes a TestBus implementation intended for integration testing.

The second part of the framework is Minor.Nijn.WebScale, which is an abstraction around Minor.Nijn, and provides a set of Attributes to annotate your classes and methods with.

## Minor.Nijn [![Nuget](https://img.shields.io/nuget/v/Minor.Nijn.Bloem.svg)](https://www.nuget.org/packages/Minor.Nijn.Bloem)
The first thing you have to do is create a `BusContext`, this can be done by using the `RabbitMQContextBuilder`. An example:

```cs
var context = new RabbitMQContextBuilder()
        .WithExchange("ExchangeName")
        .WithAddress("localhost", 5672)
        .WithCredentials("username", "password")
        .CreateContext();
```

Another way to create a `BusContext` is by using environment variables, this can be done as follows:

```cs
var context = new RabbitMQContextBuilder()
        .ReadFromEnvironmentVariables()
        .CreateContext();
```

When using environment variables, the following environment variables have to be set.

| Environment variable  | Description                                       | Type      | Default value |
| --------------------- | ------------------------------------------------- | --------- | ------------- |
| NIJN_EXCHANGE_NAME    | The name of the RabbitMQ exchange to use          | string    |               |
| NIJN_EXCHANGE_TYPE    | The exchange type to use                          | string    | topic         |
| NIJN_HOSTNAME         | The hostname of the RabbitMQ host                 | string    |               |
| NIJN_PORT             | The port the RabbitMQ host is listening on        | int       |               |
| NIJN_USERNAME         | The username used to connect to the RabbitMQ host | string    |               |
| NIJN_PASSWORD         | The password used to connect to the RabbitMQ host | string    |               |

### Using the TestBus
The Minor.Nijn framework comes with a build in TestBus which can be used to replace the RabbitMQ implementation in an integration test situation, to use the TestBus you can do the following:

```cs
// Inject this context into the test target
var context = new TestBusContextBuilder().CreateTestContext();
```

The `TestBusContext` provides you with some extra features that can come handy during a test situation, for example, you can access the declared queues, enqueue or dispatch messages.

```cs
// Get the number of messages in a queue
context.EventBus.Queues["QueueName"].MessageQueueLength;

// Get the number of declared queues
context.EventBus.QueueCount;

// Enqueue a message in a specific queue
context.EventBus.Queues["QueueName"].Enqueue(message);

// Dispatch a message
context.EventBus.DispatchMessage(message);
```

**note:** The EventBus property can be replaced with the CommandBus property to access or send a message to the command queues.

## Minor.Nijn.WebScale [![Nuget](https://img.shields.io/nuget/v/Minor.Nijn.Bloem.WebScale.svg)](https://www.nuget.org/packages/Minor.Nijn.Bloem.WebScale)
The Minor.Nijn.WebScale framework is an abstraction built on top of the Minor.Nijn framework. It provides you with a set of attributes to annotate your classes and methods with, under the hood, these attributes will be translated into event or command listeners bound to a queue on the RabbitMQ host. Creating an instance can be done by using the `MicroserviceHostBuilder`.

```cs
// Optional: creating logger factory using Microsoft Extension Logging
ILoggerFactory loggerFactory = new LoggerFactory();
loggerFactory.AddProvider(
  new ConsoleLoggerProvider(
    (text, logLevel) => logLevel >= LogLevel.Information , true));

// Create a RabbitMQ context
var context = new RabbitMQContextBuilder()
        .SetLoggerFactory(loggerFactory) // Optional
        .ReadFromEnvironmentVariables()
        .CreateContext();

// Configure the microservice host
var hostBuilder = new MicroserviceHostBuilder()
        .SetLoggerFactory(loggerFactory) // Optional
        .RegisterDependencies(services => {
            // Dependencies
        })
        .WithContext(context)
        .UseConventions()
        .ScanForExceptions();

// Create the microservice host and start listening
using (var host = hostBuilder.CreateHost())
{
    host.RegisterListeners(); // Optional: Only declare queues on RabbitMQ
    host.StartListening();
    Console.ReadKey();
}
```

**Note:** For more information about dependency injection, see: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.2 

The above example works great in a console application, but when using the framework in an ASP.NET Core application you can't make use of `Console.ReadKey`, so it's better to use a ResetEvent, for example:

```cs
// Create a manual reset event, as class variable for example
private ManualResetEvent flag = new ManualResetEvent(false);

... // Create a microservice host, see example above

// Start listening for command and events in the background
ThreadPool.QueueUserWorkItem(args => {
    using (var host = hostBuilder.CreateHost())
    {
        host.StartListening();
        flag.WaitOne();
    }
});
```

### DomainEvent and DomainCommand
`DomainEvent` and `DomainCommand` are the base classes, each event or command sent with the framework should extend one of these classes.

```cs
public class SomeEvent : DomainEvent {
    public string Payload { get; };

    public SomeEvent(string payload, string routingKey) : base(routingKey) 
    {
        Payload = payload;
    }
}
```

### Event and Command Listeners
`EventListers` can be created by using the `EventListener` and `Topic` attributes and a `CommandListener` can be created by using the `CommandListener` and `Command` attributes, for example:

```cs
[EventListener("QueueName")]
public class SomeEventListener
{
    [Topic("service.event")]
    public void EventHandlerMethod(SomeEvent evt)
    {
        // Event handler logic
    }
}


[CommandListener]
public class SomeCommandListener
{
    [Command("QueueName")]
    public long SomeCommandHandler(SomeCommand request) {
        // Command handler logic
        return 42;
    }
}
```

### Event and Command Publisher
An `EventPublisher` or `CommandPublisher` can be created in two ways, through dependency injection or by using the new keyword, for example:

```cs
// Create a publisherer with the new keyword
public class SomeServiceClass
{
    private readonly IBusContext<IConnection> _context;

    // Inject the IBusContext<IConnection>
    public SomeClass(IBusContext<IConnection> context)
    {
        _context = context;
    }

    public async void SomeMethod() {
        // Event publisher always returns void, and
        // exceptions are not possible.
        var eventPublisher  = new EventPublisher(_context);
        event.Publish(eventMessage);

        // Command publisher returns task of Type T, and
        // exceptions are possible.
        var commandPublisher = new CommandPublisher(_context);
        
        try {
            await commandPublisher.Publish<long>(commandMessage);
        }
        catch (Exception)
        {
            throw;
        }
    }
}


// Inject the command or event publisher
[CommandListener]
public class SomeEventListener
{
    private readonly IEventPublisher _publisher;

    public SomeEventListener(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    [Command("QueueName")]
    public long SomeCommandListener(SomeCommand request) {
        _publisher.Publish(eventMessage);
    }
}
```

**Note:** Event and command publisher can be injected by default into the event or command listeners.

### Exceptions
It is possible to throw an exception in a `CommandListener` method when an exception is thrown it will be serialized and returned as a response. The receiver will try to deserialize the exception and rethrow it. A custom exception can look like:

```cs
[Serializable]
public class SomeCustomException : Exception
{
    public SomeCustomException(string message) : base(message)
    {
    }

    protected SomeCustomException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
```

**Note:** It's important that the custom exception is serializable, when serializable is not used the exception will not work!

When using the `MicroserviceHostBuilder` you have the option to scan for exceptions with the `ScanForExceptions()` or `ScanForExceptions(exclusions)` methods, these methods will create an exception type dictionary which the `CommandPublisher` can use. The `CommandPublisher` will do it's best to resolve the exception type, which will happen in the following order:

1. Look in the exception type dictionary
2. Look in the calling assembly
3. Look in the `mscorlib` assembly
4. When the above doesn't work use the base class `Exception`

When the exception couldn't be created an `InvalidCastException` will be thrown.

To exclude exceptions from the exception scanning process pass a list of type `List<string>` to the `ScanForExceptions(exclusions)` method, the `MicroserviceHostBuilder` will use this list to match the namespace prefixes. For example `new list<string> { "Minor.Nijn" }` will exclude all exceptions located in namespaces starting with `Minor.Nijn`. 

### Start listening from a given timestamp
It's possible to only accept messages from a given timestamp. This can be done by using the `StartListening(long fromTimestamp)` method of the `MicroserviceHost`, for example:

```cs
... // Create a microservice host, see example above

// Start the microservice host
long fromTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

using (var host = hostBuilder.CreateHost())
{
    host.StartListening(fromTimestamp);
    Console.ReadLine();
}
```

**Note:** When using the `StartListening(long fromTimestamp)` method it's is important that you use a `long` representing the unix timestamp in milliseconds.
