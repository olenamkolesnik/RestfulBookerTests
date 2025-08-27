using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RestfulBookerTests.Utils;

public class ConfigManager
{
    private readonly IConfiguration _config;

    public ConfigManager(IConfiguration config)
    {
        _config = config;
    }

    public string BaseUrl => _config["Api:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl not configured.");
    public int MaxContentLength => _config.GetValue("Logging:MaxContentLength", 1000);
    public bool DisableContentForAuth => _config.GetValue("Logging:DisableContentForAuth", true);
    public bool EnableDetailedLogging => _config.GetValue("Logging:EnableDetailedLogging", true);

    public string Username => GetEnvironmentVariable("Username");
    public string Password => GetEnvironmentVariable("Password");

    private static string GetEnvironmentVariable(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrEmpty(value))
            throw new InvalidOperationException($"Environment variable '{key}' is not set.");
        return value;
    }
}
