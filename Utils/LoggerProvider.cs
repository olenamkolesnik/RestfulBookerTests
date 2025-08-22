using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace RestfulBookerTests.Utils
{
    //TODO:delete???
    public static class LoggerProvider
    {
        public static ILogger CreateLogger(string categoryName, string logLevel)
        {
            var level = logLevel.ToUpper() switch
            {
                "TRACE" => LogLevel.Trace,
                "DEBUG" => LogLevel.Debug,
                "INFORMATION" => LogLevel.Information,
                "WARNING" => LogLevel.Warning,
                "ERROR" => LogLevel.Error,
                "CRITICAL" => LogLevel.Critical,
                _ => LogLevel.Debug
            };

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(level)
                    .AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                    });
            });

            return loggerFactory.CreateLogger(categoryName);
        }
    }
}
