﻿namespace Minor.Nijn
{
    internal static class Constants
    {
        // General
        public const string DefaultRabbitMqExchangeType = "topic";
        public const int CommandResponseTimeoutMs = 5000;

        // Environment variables
        public const string EnvExchangeName = "NIJN_EXCHANGE_NAME";
        public const string EnvExchangeType = "NIJN_EXCHANGE_TYPE";
        public const string EnvHostname = "NIJN_HOSTNAME";
        public const string EnvPort = "NIJN_PORT";
        public const string EnvUsername = "NIJN_USERNAME";
        public const string EnvPassword = "NIJN_PASSWORD"; // NOSONAR
    }
}