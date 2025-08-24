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
        protected readonly RestClient _client;
        protected readonly ILogger _logger;
        private readonly object _tokenLock = new();
        private volatile string? _token;

        public BaseClient(string baseUrl, ILogger logger)
        {
            _client = new RestClient(baseUrl);
            _logger = logger;
        }

        public void SetToken(string token)
        {
            lock (_tokenLock)
            {
                _token = token;
            }
        }

        public string? GetToken()
        {
            lock (_tokenLock)
            {
                return _token;
            }
        }

        /// <summary>
        /// Executes a request with lightweight retry and exponential backoff
        /// </summary>
        protected async Task<(RestResponse Response, long ElapsedMs)> ExecuteAsync(
            RestRequest request,
            bool requiresAuth = true,
            int maxRetries = 3,
            CancellationToken cancellationToken = default)
        {
            if (requiresAuth && string.IsNullOrEmpty(GetToken()))
                throw new InvalidOperationException("Request requires auth but no token is set.");

            if (requiresAuth)
                request.AddOrUpdateHeader("Cookie", $"token={GetToken()}");

            if (!request.Parameters.Any(p => p.Name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)))
                request.AddHeader("Content-Type", "application/json");
            if (!request.Parameters.Any(p => p.Name.Equals("Accept", StringComparison.OrdinalIgnoreCase)))
                request.AddHeader("Accept", "application/json");

            int attempt = 0;
            var stopwatch = new Stopwatch();

            while (true)
            {
                attempt++;
                try
                {
                    stopwatch.Restart();
                    RestResponse response = await _client.ExecuteAsync(request, cancellationToken);
                    stopwatch.Stop();

                    LoggingHelper.LogRequestAndResponse(
                        _logger,
                        _client,
                        request,
                        response,
                        stopwatch.ElapsedMilliseconds,
                        GetToken()
                    );

                    if ((int)response.StatusCode >= 500 || response.StatusCode == 0)
                    {
                        if (attempt <= maxRetries)
                        {
                            int delay = 200 * attempt;
                            _logger.LogWarning(
                                "Retry {Attempt}/{MaxRetries} after {Delay}ms due to transient failure: {Reason}",
                                attempt,
                                maxRetries,
                                delay,
                                response.StatusDescription
                            );
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }
                    }

                    return (response, stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex) when (attempt <= maxRetries)
                {
                    int delay = 200 * attempt;
                    _logger.LogWarning(
                        ex,
                        "Attempt {Attempt}/{MaxRetries} failed. Retrying after {Delay}ms",
                        attempt,
                        maxRetries,
                        delay
                    );
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Executes a request and deserializes the response content to a specified type
        /// </summary>
        protected async Task<(T Data, RestResponse Raw, long ElapsedMs)> ExecuteAsync<T>(
            RestRequest request,
            string errorMessageOnDeserialize,
            bool requiresAuth = true,
            CancellationToken cancellationToken = default) where T : class
        {
            (RestResponse raw, long ms) = await ExecuteAsync(
                request,
                requiresAuth: requiresAuth,
                cancellationToken: cancellationToken
            );

            T data = JsonHelper.DeserializeSafe<T>(raw.Content, errorMessageOnDeserialize);
            return (data, raw, ms);
        }

        /// <summary>
        /// Authenticates with username/password and stores the token internally
        /// </summary>
        public async Task<(string Content, HttpStatusCode StatusCode, long ElapsedMs)> AuthenticateAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default)
        {
            var request = new RestRequest("/auth", Method.Post)
                .AddJsonBody(new { username, password });

            (RestResponse response, long elapsedMs) = await ExecuteAsync(
                request,
                requiresAuth: false,
                cancellationToken: cancellationToken
            );

            AuthResponse authResponse = JsonHelper.DeserializeSafe<AuthResponse>(
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
