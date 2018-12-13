using Microsoft.Extensions.Logging;

namespace ConsoleAppExample
{
    internal class ConsoleAppExampleLogger
    {
        public static ILoggerFactory LoggerFactory { get; set; } = new LoggerFactory();

        public static ILogger CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }
    }
}
