using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestfulBookerTests.Utils;
using RestSharp;

namespace RestfulBookerTests.Helpers
{
    public static class LoggingHelper
    {
        private const string Placeholder = "***EDITED***";

        /// <summary>
        /// Logs HTTP request and response with elapsed time, headers, and sanitized bodies.
        /// </summary>
        public static void LogRequestAndResponse(
            ILogger logger,
            RestClient client,
            RestRequest request,
            RestResponse response,
            long elapsedMs,
            string? token)
        {
            bool isAuth = request.Resource.Contains("/auth", StringComparison.OrdinalIgnoreCase);

            // Sanitize and pretty-print response content
            string sanitizedResponse = SanitizeAndPrettyPrintJson(
                response.Content,
                token,
                isAuth,
                ConfigManager.MaxContentLength
            );

            // Sanitize headers
            var sanitizedHeaders = request.Parameters
                .Where(p => p.Type == ParameterType.HttpHeader)
                .Select(p =>
                {
                    if (!string.IsNullOrEmpty(token) && p.Value?.ToString()?.Contains(token) == true)
                        return $"{p.Name}: {Placeholder}";

                    return $"{p.Name}: {p.Value}";
                });

            string headersString = sanitizedHeaders.Any()
                ? string.Join(", ", sanitizedHeaders)
                : "[None]";

            // Basic log
            logger.LogInformation(
                "HTTP {Method} {Url} => {StatusCode} in {Elapsed} ms",
                request.Method,
                client.BuildUri(request),
                response.StatusCode,
                elapsedMs
            );

            if (ConfigManager.EnableDetailedLogging)
            {
                // Log request body
                object? requestBody = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody)?.Value;
                if (requestBody != null)
                {
                    requestBody = isAuth
                        ? SanitizeAuthRequest(requestBody)
                        : SanitizeAndTruncateContent(requestBody.ToString(), token, ConfigManager.MaxContentLength);
                }

                logger.LogInformation("Detailed Logging Enabled:");
                logger.LogInformation("Request Headers: {Headers}", headersString);
                logger.LogInformation("Request Body: {Body}", requestBody ?? "[None]");
                logger.LogInformation("Response Body (sanitized): {Response}", sanitizedResponse);
            }
        }

        /// <summary>
        /// Redacts username/password from /auth request body safely.
        /// </summary>
        private static object SanitizeAuthRequest(object requestBody)
        {
            try
            {
                string json = requestBody is string s ? s : JsonConvert.SerializeObject(requestBody);
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json)
                           ?? new Dictionary<string, object>();

                if (dict.ContainsKey("username")) dict["username"] = Placeholder;
                if (dict.ContainsKey("password")) dict["password"] = Placeholder;

                return JsonConvert.SerializeObject(dict, Formatting.Indented);
            }
            catch
            {
                return Placeholder;
            }
        }

        /// <summary>
        /// Sanitizes and pretty-prints JSON response, redacts token fields.
        /// </summary>
        private static string SanitizeAndPrettyPrintJson(string? content, string? token, bool isAuth, int maxLength)
        {
            if (string.IsNullOrEmpty(content)) return "[No content]";

            try
            {
                var jsonObj = JsonConvert.DeserializeObject<object>(content);
                if (jsonObj == null) return "[Empty JSON]";

                // Redact sensitive fields
                if (isAuth && jsonObj is JObject jObjAuth && jObjAuth.ContainsKey("token"))
                {
                    jObjAuth["token"] = Placeholder;
                }
                else if (!string.IsNullOrEmpty(token) && jsonObj is JObject jObjToken)
                {
                    foreach (var prop in jObjToken.Properties().Where(p => p.Value.ToString()?.Contains(token) ?? false))
                        prop.Value = Placeholder;
                }

                // Pretty-print JSON
                string pretty = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);

                // Truncate if too long
                return pretty.Length > maxLength ? pretty.Substring(0, maxLength) + "..." : pretty;
            }
            catch
            {
                // Fallback: raw truncated content
                string sanitized = !string.IsNullOrEmpty(token) ? content.Replace(token, Placeholder) : content;
                return sanitized.Length > maxLength ? sanitized.Substring(0, maxLength) + "..." : sanitized;
            }
        }

        /// <summary>
        /// Truncates and sanitizes non-JSON content.
        /// </summary>
        public static string SanitizeAndTruncateContent(string? content, string? token, int maxLength)
        {
            if (string.IsNullOrEmpty(content)) return "[No content]";

            string sanitized = !string.IsNullOrEmpty(token) ? content.Replace(token, Placeholder) : content;
            return sanitized.Length > maxLength ? sanitized.Substring(0, maxLength) + "..." : sanitized;
        }
    }
}
