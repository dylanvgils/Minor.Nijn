using System;

namespace Minor.Nijn.Helpers
{
    public static class EnvironmentHelper
    {
        public static void GetValue(string key, Action<string> action, string defaultValue = null)
        {
            var value = Environment.GetEnvironmentVariable(key);

            if (value != null)
            {
                action(value);
                return;
            }

            if (defaultValue != null)
            {
                action(defaultValue);
                return;
            }

            throw new ArgumentException($"Invalid environment variable with key: {key}");
        }
    }
}
