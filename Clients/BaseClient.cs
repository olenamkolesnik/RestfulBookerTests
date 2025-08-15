using Microsoft.Extensions.Logging;
using RestSharp;
using RestfulBookerTests.Helpers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RestfulBookerTests.Clients
{
    public class BaseClient : IDisposable
    {
        private readonly string _baseUrl;
        protected readonly ILogger _logger;
        private string? _token;

        public BaseClient(string baseUrl, ILogger logger)
        {
            _baseUrl = baseUrl;
            _logger = logger;
        }

        public void SetToken(string token) => _token = token;

        public async Task<string> AuthenticateAsync(string username, string password)
        {
            var client = new RestClient(_baseUrl);
            var request = new RestRequest("/auth", Method.Post)
                .AddJsonBody(new { username, password });

            LoggingHelper.LogRequest(_logger, Method.Post, "/auth", new { username, password });

            var stopwatch = Stopwatch.StartNew();
            var response = await client.ExecuteAsync<AuthResponse>(request);
            stopwatch.Stop();

            await LoggingHelper.LogResponseAsync(_logger, response, stopwatch);

            if (!response.IsSuccessful || response.Data is null)
                throw new InvalidOperationException($"Authentication failed: {response.Content}");

            _token = response.Data.Token;
            return _token;
        }

        protected RestClient GetClient()
        {
            var client = new RestClient(_baseUrl);
            if (!string.IsNullOrEmpty(_token))
                client.AddDefaultHeader("Authorization", $"Bearer {_token}");
            return client;
        }

        protected async Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request) where T : class
        {
            object? payload = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody)?.Value;
            LoggingHelper.LogRequest(_logger, request.Method, request.Resource, payload);

            var client = GetClient();
            var stopwatch = Stopwatch.StartNew();
            var response = await client.ExecuteAsync<T>(request);
            stopwatch.Stop();

            await LoggingHelper.LogResponseAsync(_logger, response, stopwatch);

            return response;
        }

        protected async Task<RestResponse> ExecuteAsync(RestRequest request)
        {
            object? payload = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody)?.Value;
            LoggingHelper.LogRequest(_logger, request.Method, request.Resource, payload);

            var client = GetClient();
            var stopwatch = Stopwatch.StartNew();
            var response = await client.ExecuteAsync(request);
            stopwatch.Stop();

            await LoggingHelper.LogResponseAsync(_logger, response, stopwatch);

            return response;
        }

        public void Dispose()
        {
            // RestClient doesn't require disposal, but pattern is here if needed later
        }
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
