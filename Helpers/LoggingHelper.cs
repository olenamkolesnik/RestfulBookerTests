using Microsoft.Extensions.Logging;
using RestSharp;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RestfulBookerTests.Helpers
{
    public static class LoggingHelper
    {
        public static void LogRequest(ILogger logger, Method method, string endpoint)
        {
            logger.LogInformation("Sending {Method} request to {Endpoint}", method, endpoint);
        }

        public static void LogRequest<T>(ILogger logger, Method method, string endpoint, T? payload)
            where T : class
        {
            if (payload != null)
                logger.LogInformation("Sending {Method} request to {Endpoint} with payload: {@Payload}", method, endpoint, payload);
            else
                logger.LogInformation("Sending {Method} request to {Endpoint}", method, endpoint);
        }

        public static async Task LogResponseAsync(ILogger logger, RestResponse response, Stopwatch? stopwatch = null)
        {
            stopwatch ??= Stopwatch.StartNew();
            stopwatch.Stop();

            // Log headers
            foreach (var header in response.Headers)
                logger.LogDebug("Response Header: {Name}: {Value}", header.Name, header.Value);

            var body = response.Content ?? string.Empty;
            var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

            if (response.IsSuccessful)
                logger.LogDebug("Response {StatusCode} ({StatusDescription}) in {ElapsedMilliseconds}ms: {Body}",
                                response.StatusCode, response.StatusDescription, elapsedMs, body);
            else if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                logger.LogWarning("Client error {StatusCode} ({StatusDescription}) in {ElapsedMilliseconds}ms: {Body}",
                                  response.StatusCode, response.StatusDescription, elapsedMs, body);
            else if ((int)response.StatusCode >= 500)
                logger.LogError("Server error {StatusCode} ({StatusDescription}) in {ElapsedMilliseconds}ms: {Body}",
                                response.StatusCode, response.StatusDescription, elapsedMs, body);
            else
                logger.LogInformation("Unexpected response {StatusCode} ({StatusDescription}) in {ElapsedMilliseconds}ms: {Body}",
                                      response.StatusCode, response.StatusDescription, elapsedMs, body);

            await Task.CompletedTask;
        }

        public static async Task LogResponseAsync<T>(ILogger logger, RestResponse<T> response, Stopwatch? stopwatch = null)
        {
            stopwatch ??= Stopwatch.StartNew();
            stopwatch.Stop();

            foreach (var header in response.Headers)
                logger.LogDebug("Response Header: {Name}: {Value}", header.Name, header.Value);

            var body = response.Content ?? string.Empty;
            var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

            if (response.IsSuccessful)
                logger.LogDebug("Response {StatusCode} ({StatusDescription}) in {ElapsedMilliseconds}ms: {Body}",
                                response.StatusCode, response.StatusDescription, elapsedMs, body);
            else
                logger.LogError("Response {StatusCode} ({StatusDescription}) in {ElapsedMilliseconds}ms: {Body}",
                                response.StatusCode, response.StatusDescription, elapsedMs, body);

            await Task.CompletedTask;
        }
    }
}
