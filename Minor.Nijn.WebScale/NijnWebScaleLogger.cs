using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Minor.Nijn.WebScale
{
    internal static class NijnWebScaleLogger
    {
        public static ILoggerFactory LoggerFactory { get; set; }

        static NijnWebScaleLogger()
        {
            LoggerFactory = new LoggerFactory()
                .AddSerilog(new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger()
                );
        }

        public static ILogger CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }
    }
}
