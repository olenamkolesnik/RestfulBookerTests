using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestfulBookerTests.Clients;
using RestfulBookerTests.Helpers;
using RestfulBookerTests.Utils;

namespace RestfulBookerTests.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRestfulBookerServices(this IServiceCollection services)
    {
        // 1. IConfiguration
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
        IConfiguration config = builder.Build();
        services.AddSingleton(config);

        // 2. ConfigManager
        services.AddSingleton<ConfigManager>();

        // 3. LoggingHelper
        services.AddSingleton<LoggingHelper>();

        // 4. Logging
        services.AddLogging(loggingBuilder =>
        {
            var logLevel = config.GetValue("Api:LogLevel", "Debug");
            if (!Enum.TryParse<LogLevel>(logLevel, true, out var level))
                level = LogLevel.Debug;

            loggingBuilder.SetMinimumLevel(level);
            loggingBuilder.AddConsole();
        });

        // 5. Clients (scoped per test)
        services.AddScoped<BaseClient>();
        services.AddScoped<BookingClient>();

        return services;
    }
}
