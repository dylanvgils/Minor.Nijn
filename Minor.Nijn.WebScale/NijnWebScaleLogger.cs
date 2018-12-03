using Microsoft.Extensions.Logging;

namespace Minor.Nijn.WebScale
{
    internal static class NijnWebScaleLogger
    {
        public static ILoggerFactory DefaultFactory { get; } = new LoggerFactory();
        public static ILoggerFactory LoggerFactory { get; set; }

        public static ILogger CreateLogger<T>()
        {
            if (LoggerFactory == null)
            {
                return DefaultFactory.CreateLogger<T>();
            }

            return LoggerFactory.CreateLogger<T>();
        }
    }
}
