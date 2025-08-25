using Reqnroll;
using RestfulBookerTests.Models;

namespace RestfulBookerTests.Extensions
{
    public static class SchemaValidationExtensions
    {
        // Schema validation data storage
        public static void SetBookingSchema(this ScenarioContext context, Booking booking)
        {
            context["BookingSchema"] = booking;
        }

        public static Booking GetBookingSchema(this ScenarioContext context)
        {
            if (!context.TryGetValue<Booking>("BookingSchema", out var booking) || booking == null)
            {
                throw new InvalidOperationException("BookingSchema not found in ScenarioContext.");
            }
            return booking;
        }

        public static void SetBookingCreatedResponseSchema(this ScenarioContext context, BookingCreatedResponse response)
        {
            context["BookingCreatedResponseSchema"] = response;
        }

        public static BookingCreatedResponse GetBookingCreatedResponseSchema(this ScenarioContext context)
        {
            if (!context.TryGetValue<BookingCreatedResponse>("BookingCreatedResponseSchema", out var response) || response == null)
            {
                throw new InvalidOperationException("BookingCreatedResponseSchema not found in ScenarioContext.");
            }
            return response;
        }

        public static void SetAuthResponseSchema(this ScenarioContext context, AuthResponse response)
        {
            context["AuthResponseSchema"] = response;
        }

        public static AuthResponse GetAuthResponseSchema(this ScenarioContext context)
        {
            if (!context.TryGetValue<AuthResponse>("AuthResponseSchema", out var response) || response == null)
            {
                throw new InvalidOperationException("AuthResponseSchema not found in ScenarioContext.");
            }
            return response;
        }

        // Generic schema validation helper
        public static T GetSchemaData<T>(this ScenarioContext context) where T : class
        {
            var key = $"{typeof(T).Name}Schema";
            if (!context.TryGetValue<T>(key, out var data) || data == null)
            {
                throw new InvalidOperationException($"Schema data for {typeof(T).Name} not found in ScenarioContext.");
            }
            return data;
        }

        public static void SetSchemaData<T>(this ScenarioContext context, T data) where T : class
        {
            var key = $"{typeof(T).Name}Schema";
            context[key] = data;
        }
    }
}