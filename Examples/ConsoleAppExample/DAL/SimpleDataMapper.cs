using Microsoft.Extensions.Logging;

namespace ConsoleAppExample.DAL
{
    public class SimpleDataMapper : IDataMapper<string, long>
    {
        private readonly ILogger logger;

        public SimpleDataMapper()
        {
            logger = ConsoleAppExampleLogger.CreateLogger<SimpleDataMapper>();
        }

        public void Save(string item)
        {
            logger.LogInformation("Hello from SimpleDataMapper, i was called with string: {0}", item);
        }
    }
}
