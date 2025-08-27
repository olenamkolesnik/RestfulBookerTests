using Microsoft.Extensions.Logging;
using RestfulBookerTests.Utils;
using RestSharp;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RestfulBookerTests.Helpers;

public class LoggingHelper
{
    private const string Placeholder = "***EDITED***";
    private readonly ConfigManager _config;
    private static readonly string[] SensitiveFields = { "token", "password", "username" };

    public LoggingHelper(ConfigManager config)
    {
        _config = config;
    }

    public void LogRequestAndResponse(
        ILogger logger,
        RestClient client,
        RestRequest request,
        RestResponse response,
        long elapsedMs,
        string? token)
    {
        bool isAuth = request.Resource.Contains("/auth", StringComparison.OrdinalIgnoreCase);

        logger.LogInformation("HTTP {Method} {Url} => {StatusCode} in {Elapsed} ms",
            request.Method,
            client.BuildUri(request),
            response.StatusCode,
            elapsedMs);

        if (!_config.EnableDetailedLogging && response.IsSuccessful)
            return;

        string sanitizedResponse = SanitizeAndPrettyPrintJson(response.Content, token, isAuth, _config.MaxContentLength);

        var sanitizedHeaders = request.Parameters
            .Where(p => p.Type == ParameterType.HttpHeader)
            .Select(p =>
            {
                if (!string.IsNullOrEmpty(token) && p.Value?.ToString()?.Contains(token) == true)
                    return $"{p.Name}: {Placeholder}";
                return $"{p.Name}: {p.Value}";
            });
        string headersString = sanitizedHeaders.Any() ? string.Join(", ", sanitizedHeaders) : "[None]";

        object? requestBody = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody)?.Value;
        string sanitizedRequestBody = requestBody != null
            ? (isAuth ? SanitizeAuthRequest(requestBody) : SanitizeAndTruncateContent(requestBody.ToString(), token, _config.MaxContentLength))
            : "[None]";

        logger.LogInformation("Detailed Logging:");
        logger.LogInformation("Request Headers: {Headers}", headersString);
        logger.LogInformation("Request Body: {Body}", sanitizedRequestBody);
        logger.LogInformation("Response Body (sanitized): {Response}", sanitizedResponse);
    }

    private static string SanitizeAuthRequest(object requestBody)
    {
        try
        {
            JsonNode? node = JsonNode.Parse(requestBody is string s ? s : JsonSerializer.Serialize(requestBody));
            if (node is JsonObject obj)
            {
                foreach (var key in SensitiveFields)
                    if (obj.ContainsKey(key))
                        obj[key] = Placeholder;
            }
            return node?.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) ?? Placeholder;
        }
        catch
        {
            return Placeholder;
        }
    }

    private static string SanitizeAndPrettyPrintJson(string? content, string? token, bool isAuth, int maxLength)
    {
        if (string.IsNullOrEmpty(content))
            return "[No content]";

        try
        {
            JsonNode? root = JsonNode.Parse(content);
            if (root == null) return "[Empty JSON]";

            RedactSensitiveFields(root, token);

            string pretty = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            return pretty.Length > maxLength ? pretty.Substring(0, maxLength) + "..." : pretty;
        }
        catch
        {
            string sanitized = !string.IsNullOrEmpty(token) ? content.Replace(token, Placeholder) : content;
            return sanitized.Length > maxLength ? sanitized.Substring(0, maxLength) + "..." : sanitized;
        }
    }

    private static void RedactSensitiveFields(JsonNode node, string? token)
    {
        if (node is JsonObject obj)
        {
            foreach (var key in obj.Select(kv => kv.Key).ToList())
            {
                if (SensitiveFields.Contains(key, StringComparer.OrdinalIgnoreCase))
                    obj[key] = Placeholder;
                else if (obj[key] != null)
                    RedactSensitiveFields(obj[key]!, token);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
                if (item != null) RedactSensitiveFields(item, token);
        }
        else if (!string.IsNullOrEmpty(token) && node is JsonValue value && value.ToString().Contains(token))
        {
            var parent = node.Parent;
            if (parent is JsonObject parentObj)
            {
                var kv = parentObj.FirstOrDefault(p => p.Value == node);
                if (kv.Key != null)
                    parentObj[kv.Key] = Placeholder;
            }
        }
    }

    public static string SanitizeAndTruncateContent(string? content, string? token, int maxLength)
    {
        if (string.IsNullOrEmpty(content))
            return "[No content]";

        string sanitized = !string.IsNullOrEmpty(token) ? content.Replace(token, Placeholder) : content;
        return sanitized.Length > maxLength ? sanitized.Substring(0, maxLength) + "..." : sanitized;
    }
}
