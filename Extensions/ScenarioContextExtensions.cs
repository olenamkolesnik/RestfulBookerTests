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

        // Specific typed methods for better IntelliSense and discoverability
        public static BookingClient GetBookingClient(this ScenarioContext context)
        {
            return context.GetClient<BookingClient>();
        }

        public static BaseClient GetBaseClient(this ScenarioContext context)
        {
            return context.GetClient<BaseClient>();
        }

        // Optional: Methods for other common scenario data
        public static T GetData<T>(this ScenarioContext context, string key) where T : class
        {
            if (!context.TryGetValue<T>(key, out var data) || data == null)
            {
                throw new InvalidOperationException($"Data with key '{key}' not found in ScenarioContext.");
            }
            return data;
        }

        public static void SetData<T>(this ScenarioContext context, string key, T data) where T : class
        {
            context[key] = data;
        }

        // Strongly typed methods for all scenario data

        // Current Booking
        public static void SetCurrentBooking(this ScenarioContext context, RestfulBookerTests.Models.Booking booking)
        {
            context[ScenarioKeys.CurrentBooking] = booking;
        }

        public static RestfulBookerTests.Models.Booking GetCurrentBooking(this ScenarioContext context)
        {
            return context.GetData<RestfulBookerTests.Models.Booking>(ScenarioKeys.CurrentBooking);
        }

        // Updated Booking
        public static void SetUpdatedBooking(this ScenarioContext context, RestfulBookerTests.Models.Booking booking)
        {
            context[ScenarioKeys.UpdatedBooking] = booking;
        }

        public static RestfulBookerTests.Models.Booking GetUpdatedBooking(this ScenarioContext context)
        {
            return context.GetData<RestfulBookerTests.Models.Booking>(ScenarioKeys.UpdatedBooking);
        }

        // Booking Created Response
        public static void SetBookingCreatedResponse(this ScenarioContext context, RestfulBookerTests.Models.BookingCreatedResponse response)
        {
            context[ScenarioKeys.BookingCreatedResponse] = response;
        }

        public static RestfulBookerTests.Models.BookingCreatedResponse GetBookingCreatedResponse(this ScenarioContext context)
        {
            return context.GetData<RestfulBookerTests.Models.BookingCreatedResponse>(ScenarioKeys.BookingCreatedResponse);
        }

        // Created Booking
        public static void SetCreatedBooking(this ScenarioContext context, RestfulBookerTests.Models.Booking booking)
        {
            context[ScenarioKeys.CreatedBooking] = booking;
        }

        public static RestfulBookerTests.Models.Booking GetCreatedBooking(this ScenarioContext context)
        {
            return context.GetData<RestfulBookerTests.Models.Booking>(ScenarioKeys.CreatedBooking);
        }

        // Retrieved Booking
        public static void SetRetrievedBooking(this ScenarioContext context, RestfulBookerTests.Models.Booking booking)
        {
            context[ScenarioKeys.RetrievedBooking] = booking;
        }

        public static RestfulBookerTests.Models.Booking GetRetrievedBooking(this ScenarioContext context)
        {
            return context.GetData<RestfulBookerTests.Models.Booking>(ScenarioKeys.RetrievedBooking);
        }

        // Last Status Code
        public static void SetLastStatusCode(this ScenarioContext context, System.Net.HttpStatusCode statusCode)
        {
            context[ScenarioKeys.LastStatusCode] = statusCode;
        }

        public static System.Net.HttpStatusCode GetLastStatusCode(this ScenarioContext context)
        {
            if (!context.TryGetValue<System.Net.HttpStatusCode>(ScenarioKeys.LastStatusCode, out var statusCode))
            {
                throw new InvalidOperationException("LastStatusCode not found in ScenarioContext.");
            }
            return statusCode;
        }

        // Last Elapsed Ms
        public static void SetLastElapsedMs(this ScenarioContext context, long elapsedMs)
        {
            context[ScenarioKeys.LastElapsedMs] = elapsedMs;
        }

        public static long GetLastElapsedMs(this ScenarioContext context)
        {
            if (!context.TryGetValue<long>(ScenarioKeys.LastElapsedMs, out var elapsedMs))
            {
                throw new InvalidOperationException("LastElapsedMs not found in ScenarioContext.");
            }
            return elapsedMs;
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