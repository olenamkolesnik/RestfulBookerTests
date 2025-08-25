using Reqnroll;
using RestfulBookerTests.Clients;
using RestfulBookerTests.Helpers;

namespace RestfulBookerTests.Extensions
{
    public static class ScenarioContextExtensions
    {
        // Generic method for any client type
        public static T GetClient<T>(this ScenarioContext context) where T : class
        {
            var key = typeof(T).Name;
            if (!context.TryGetValue<T>(key, out var client) || client == null)
            {
                throw new InvalidOperationException($"{typeof(T).Name} not found in ScenarioContext. Make sure it's initialized in TestHooks.");
            }
            return client;
        }

        // Generic method to set clients
        public static void SetClient<T>(this ScenarioContext context, T client) where T : class
        {
            var key = typeof(T).Name;
            context[key] = client;
        }
      
        public static T GetData<T>(this ScenarioContext context, string key)
        {
            if (!context.TryGetValue<T>(key, out var data) || data == null)
            {
                throw new InvalidOperationException($"Data with key '{key}' not found in ScenarioContext.");
            }
            return data;
        }

        public static void SetData<T>(this ScenarioContext context, string key, T data)
        {
            context[key] = data;
        }

        // Logger (keeping this for completeness)
        public static void SetLogger(this ScenarioContext context, Microsoft.Extensions.Logging.ILogger logger)
        {
            context["Logger"] = logger;
        }

        public static Microsoft.Extensions.Logging.ILogger GetLogger(this ScenarioContext context)
        {
            if (!context.TryGetValue<Microsoft.Extensions.Logging.ILogger>("Logger", out var logger) || logger == null)
            {
                throw new InvalidOperationException("Logger not found in ScenarioContext.");
            }
            return logger;
        }

        // Optional: Try methods that return bool instead of throwing exceptions
        public static bool TryGetCurrentBooking(this ScenarioContext context, out RestfulBookerTests.Models.Booking booking)
        {
            return context.TryGetValue(ScenarioKeys.CurrentBooking, out booking) && booking != null;
        }

        public static bool TryGetBookingCreatedResponse(this ScenarioContext context, out RestfulBookerTests.Models.BookingCreatedResponse response)
        {
            return context.TryGetValue(ScenarioKeys.BookingCreatedResponse, out response) && response != null;
        }

        public static bool TryGetLastStatusCode(this ScenarioContext context, out System.Net.HttpStatusCode statusCode)
        {
            return context.TryGetValue(ScenarioKeys.LastStatusCode, out statusCode);
        }

        public static bool TryGetLastElapsedMs(this ScenarioContext context, out long elapsedMs)
        {
            return context.TryGetValue(ScenarioKeys.LastElapsedMs, out elapsedMs);
        }
    }
}