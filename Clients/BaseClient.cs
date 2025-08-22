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
        private string? _token;

        public BaseClient(string baseUrl, ILogger logger)
        {
            _client = new RestClient(baseUrl);
            _logger = logger;
        }

        public void SetToken(string token) => _token = token;
        public string? GetToken() => _token;

        /// <summary>
        /// Executes a request and returns the raw RestResponse along with elapsed milliseconds.
        /// </summary>
        protected async Task<(RestResponse Response, long ElapsedMs)> ExecuteAsync(
    RestRequest request,
    bool requiresAuth = true,
    CancellationToken cancellationToken = default)
        {
            if (requiresAuth && string.IsNullOrEmpty(_token))
                throw new InvalidOperationException("Request requires auth but no token is set. Call AuthenticateAsync or SetToken first.");

            // Add auth header if required
            if (requiresAuth)
                request.AddOrUpdateHeader("Cookie", $"token={_token}");

            // Always ensure JSON headers
            if (!request.Parameters.Any(p => p.Name.Equals("Content-Type", System.StringComparison.OrdinalIgnoreCase)))
                request.AddHeader("Content-Type", "application/json");
            if (!request.Parameters.Any(p => p.Name.Equals("Accept", System.StringComparison.OrdinalIgnoreCase)))
                request.AddHeader("Accept", "application/json");

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.ExecuteAsync(request, cancellationToken);
            stopwatch.Stop();

            // Log headers, body, response, elapsed time; hide sensitive info for /auth
            LoggingHelper.LogRequestAndResponse(_logger, _client, request, response, stopwatch.ElapsedMilliseconds, _token);

            return (response, stopwatch.ElapsedMilliseconds);
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
        /// Authenticates with username/password and stores the token internally.
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

            _token = authResponse.Token;

            return (response.Content ?? string.Empty, response.StatusCode, elapsedMs);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
