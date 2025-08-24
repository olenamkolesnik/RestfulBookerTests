using Microsoft.Extensions.Logging;
using RestSharp;

namespace RestfulBookerTests.Clients
{
    public class HealthCheckClient : BaseClient
    {        public HealthCheckClient(string baseUrl, ILogger logger)
             : base(baseUrl, logger)
        {
        }
        /*
        /// <summary>
        /// Ping the server. No authentication required.
        /// </summary>
        public async Task<(RestResponse Response, long ElapsedMs)> PingAsync(CancellationToken cancellationToken = default)
        {
            var request = new RestRequest("/ping", Method.Get);

            var (response, elapsedMs) = await ExecuteAsync(request, requiresAuth: false, cancellationToken);

            return (response, elapsedMs);
        }*/
    }
}
