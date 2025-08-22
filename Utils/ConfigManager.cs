using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RestfulBookerTests.Utils
{
    public static class ConfigManager
    {
        private static readonly IConfigurationRoot _config;

        static ConfigManager()
        {
            DotNetEnv.Env.Load();

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            _config = builder.Build();
        }

        public static string BaseUrl => _config["Api:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl not configured.");
        public static LogLevel LogLevel
        {
            get
            {
                var logLevelString = _config["Api:LogLevel"];
                if (string.IsNullOrEmpty(logLevelString))
                    return LogLevel.Debug; // Default fallback
                if (!Enum.TryParse<LogLevel>(logLevelString, true, out var logLevel))
                    throw new InvalidOperationException($"Invalid LogLevel value: '{logLevelString}'.");
                return logLevel;
            }
        }
        public static string Username => GetEnvironmentVariable("Username");
        public static string Password => GetEnvironmentVariable("Password");

        private static string GetEnvironmentVariable(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException($"Environment variable '{key}' is not set.");
            return value;
        }
    }
}
