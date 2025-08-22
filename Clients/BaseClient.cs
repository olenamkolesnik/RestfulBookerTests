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
        /// Core executor with logging and optional auth. Returns raw response + elapsed ms.
        /// </summary>
        protected async Task<(RestResponse Response, long ElapsedMs)> ExecuteAsync(
            RestRequest request, 
            bool requiresAuth = true, 
            CancellationToken cancellationToken = default)
        {
            if (requiresAuth)
            {
                if (string.IsNullOrEmpty(_token))                    
                    throw new InvalidOperationException("Request requires auth but no token is set. Call AuthenticateAsync or SetToken first.");

                request.AddOrUpdateHeader("Cookie", $"token={_token}"); // Restful Booker style
            }

            _logger.LogInformation("Sending {Method} request to {Resource}", request.Method, request.Resource);

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.ExecuteAsync(request, cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation(
                "Received response ({StatusCode}) in {Elapsed} ms. Content: {Content}",
                response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                response.Content);

            return (response, stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Typed executor that deserializes to T and still returns raw response + elapsed ms.
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
        /// Authenticates against /auth and stores the token for subsequent requests.
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
            // Nothing to dispose right now, but implement IDisposable for future extensibility.
        }
    }
}
