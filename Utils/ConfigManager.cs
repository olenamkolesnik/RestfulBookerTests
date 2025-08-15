using Microsoft.Extensions.Configuration;

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
        public static string LogLevel => _config["Api:LogLevel"] ?? "DEBUG";
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
