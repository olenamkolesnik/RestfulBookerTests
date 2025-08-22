using Microsoft.Extensions.Logging;
using RestfulBookerTests.Helpers;
using RestfulBookerTests.Models;
using RestSharp;
using System.Diagnostics;
using System.Net;

namespace RestfulBookerTests.Clients
{
    public class BaseClient : IDisposable
    {
        private readonly RestClient _client;
        protected readonly ILogger _logger;
        private readonly object _tokenLock = new();
        private volatile string? _token;

        public BaseClient(string baseUrl, ILogger logger)
        {
            _client = new RestClient(baseUrl);
            _logger = logger;
        }

        /// <summary>
        /// Safely sets the authentication token.
        /// </summary>
        public void SetToken(string token)
        {
            lock (_tokenLock)
            {
                _token = token;
            }
        }

        /// <summary>
        /// Retrieves the current token (thread-safe).
        /// </summary>
        public string? GetToken()
        {
            lock (_tokenLock)
            {
                return _token;
            }
        }

        /// <summary>
        /// Executes a request and returns the raw RestResponse along with elapsed milliseconds.
        /// Includes error handling and retries for transient failures.
        /// </summary>
        protected async Task<(RestResponse Response, long ElapsedMs)> ExecuteAsync(
            RestRequest request,
            bool requiresAuth = true,
            CancellationToken cancellationToken = default)
        {
            if (requiresAuth && string.IsNullOrEmpty(GetToken()))
                throw new InvalidOperationException("Request requires auth but no token is set. Call AuthenticateAsync or SetToken first.");

            // Add auth header if required
            if (requiresAuth)
                request.AddOrUpdateHeader("Cookie", $"token={GetToken()}");

            // Ensure JSON headers
            if (!request.Parameters.Any(p => p.Name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)))
                request.AddHeader("Content-Type", "application/json");
            if (!request.Parameters.Any(p => p.Name.Equals("Accept", StringComparison.OrdinalIgnoreCase)))
                request.AddHeader("Accept", "application/json");
                        
            int maxRetries = 3;
            int attempt = 0;
            Exception? lastException = null;

            while (attempt < maxRetries)
            {
                try
                {
                    attempt++;
                    var stopwatch = Stopwatch.StartNew();
                    var response = await _client.ExecuteAsync(request, cancellationToken);
                    stopwatch.Stop();

                    // Log after execution
                    LoggingHelper.LogRequestAndResponse(_logger, _client, request, response, stopwatch.ElapsedMilliseconds, GetToken());

                    // Check for network or server errors
                    if (response.StatusCode == 0 || (int)response.StatusCode >= 500)
                    {
                        throw new HttpRequestException($"Server error or network failure: {(int)response.StatusCode} {response.StatusDescription}");
                    }

                    return (response, stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "Attempt {Attempt} failed. Retrying...", attempt);
                    await Task.Delay(500 * attempt, cancellationToken); // exponential backoff
                }
            }

            throw new HttpRequestException($"Request failed after {maxRetries} attempts.", lastException);
        }

        /// <summary>
        /// Executes a request and deserializes the response content to the specified type.
        /// </summary>
        protected async Task<(T Data, RestResponse Raw, long ElapsedMs)> ExecuteAsync<T>(
            RestRequest request,
            string errorMessageOnDeserialize,
            bool requiresAuth = true,
            CancellationToken cancellationToken = default) where T : class
        {
            var (raw, ms) = await ExecuteAsync(request, requiresAuth, cancellationToken);
            var data = JsonHelper.DeserializeSafe<T>(raw.Content, errorMessageOnDeserialize);
            return (data, raw, ms);
        }

        /// <summary>
        /// Authenticates with username/password and stores the token internally (thread-safe).
        /// </summary>
        public async Task<(string Content, HttpStatusCode StatusCode, long ElapsedMs)> AuthenticateAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default)
        {
            var request = new RestRequest("/auth", Method.Post)
                .AddJsonBody(new { username, password });

            var (response, elapsedMs) = await ExecuteAsync(request, requiresAuth: false, cancellationToken);

            var authResponse = JsonHelper.DeserializeSafe<AuthResponse>(
                response.Content,
                "Failed to authenticate."
            );

            SetToken(authResponse.Token);

            return (response.Content ?? string.Empty, response.StatusCode, elapsedMs);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
