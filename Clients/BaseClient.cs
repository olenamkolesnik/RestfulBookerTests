using Microsoft.Extensions.Logging;
using RestfulBookerTests.Helpers;
using RestfulBookerTests.Models;
using RestfulBookerTests.Utils;
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
        private string? _token;
        private DateTime? _tokenExpiry; // UTC

        public BaseClient(string baseUrl, ILogger logger)
        {
            _client = new RestClient(baseUrl);
            _logger = logger;
        }

        public void SetToken(string token, int expiresInMinutes = 5)
        {
            lock (_tokenLock)
            {
                _token = token;
                _tokenExpiry = DateTime.UtcNow.AddMinutes(expiresInMinutes);
            }
        }

        public string? GetToken()
        {
            lock (_tokenLock)
            {
                return _token;
            }
        }

        private bool TokenExpired()
        {
            lock (_tokenLock)
            {
                if (!_tokenExpiry.HasValue) return true;
                return DateTime.UtcNow >= _tokenExpiry.Value;
            }
        }

        protected async Task<(RestResponse Response, long ElapsedMs)> ExecuteAsync(
            RestRequest request,
            bool requiresAuth = true,
            CancellationToken cancellationToken = default)
        {
            // Lazy auth
            if (requiresAuth)
            {
                if (string.IsNullOrEmpty(GetToken()) || TokenExpired())
                {
                    _logger.LogInformation("Token missing or expired. Performing lazy authentication...");
                    await AuthenticateAsync(ConfigManager.Username, ConfigManager.Password, cancellationToken);
                }

                request.AddOrUpdateHeader("Cookie", $"token={GetToken()}");
            }

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

                    LoggingHelper.LogRequestAndResponse(_logger, _client, request, response, stopwatch.ElapsedMilliseconds, GetToken());

                    if (response.StatusCode == 0 || (int)response.StatusCode >= 500)
                        throw new HttpRequestException($"Server error or network failure: {(int)response.StatusCode} {response.StatusDescription}");

                    return (response, stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "Attempt {Attempt} failed. Retrying...", attempt);
                    await Task.Delay(500 * attempt, cancellationToken);
                }
            }

            throw new HttpRequestException($"Request failed after {maxRetries} attempts.", lastException);
        }

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

            SetToken(authResponse.Token, expiresInMinutes: 5); // adjust expiry if needed

            return (response.Content ?? string.Empty, response.StatusCode, elapsedMs);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
