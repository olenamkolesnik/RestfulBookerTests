using Microsoft.Extensions.Configuration;

namespace RestfulBookerTests.Utils;

public class ConfigManager
{
    private readonly IConfiguration _config;

    public ConfigManager(IConfiguration config)
    {
        _config = config;
    }

    // API base URL
    public string BaseUrl => _config["Api:BaseUrl"]
        ?? throw new InvalidOperationException("BaseUrl not configured.");

    // Logging options
    public int MaxContentLength => _config.GetValue("Logging:MaxContentLength", 1000);
    public bool DisableContentForAuth => _config.GetValue("Logging:DisableContentForAuth", true);
    public bool EnableDetailedLogging => _config.GetValue("Logging:EnableDetailedLogging", true);

    // Retry / timeout settings
    public int MaxRetries => _config.GetValue("Http:MaxRetries", 3);
    public int RetryDelayMs => _config.GetValue("Http:RetryDelayMs", 500);

    // Authentication
    public string Username => GetEnvironmentVariable("Username");
    public string Password => GetEnvironmentVariable("Password");

    //DB 
    public string DbConnectionString => _config["Database:ConnectionString"]
    ?? throw new InvalidOperationException("Database connection string not configured.");


    private static string GetEnvironmentVariable(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrEmpty(value))
            throw new InvalidOperationException($"Environment variable '{key}' is not set.");
        return value;
    }
}
