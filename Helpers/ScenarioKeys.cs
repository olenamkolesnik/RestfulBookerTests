namespace RestfulBookerTests.Helpers
{
    /// <summary>
    /// Constants for ScenarioContext keys to ensure consistency across the application.
    /// These keys are now primarily used internally by typed extension methods.
    /// Consider using the typed extension methods in ScenarioContextExtensions instead of direct key access.
    /// </summary>
    public static class ScenarioKeys
    {
        #region Client Keys
        /// <summary>Key for storing BookingClient in ScenarioContext</summary>
        public const string BookingClient = "BookingClient";

        /// <summary>Key for storing BaseClient in ScenarioContext</summary>
        public const string BaseClient = "BaseClient";

        /// <summary>Key for storing Logger in ScenarioContext</summary>
        public const string Logger = "Logger";
        #endregion

        #region Booking Data Keys
        /// <summary>Key for storing the current booking being processed</summary>
        public const string CurrentBooking = "CurrentBooking";

        /// <summary>Key for storing updated booking details for update operations</summary>
        public const string UpdatedBooking = "UpdatedBooking";

        /// <summary>Key for storing the booking created response (includes BookingId)</summary>
        public const string BookingCreatedResponse = "BookingCreatedResponse";

        /// <summary>Key for storing the created booking details</summary>
        public const string CreatedBooking = "CreatedBooking";

        /// <summary>Key for storing retrieved booking details from GET operations</summary>
        public const string RetrievedBooking = "RetrievedBooking";
        #endregion

        #region Response Metadata Keys
        /// <summary>Key for storing the last HTTP status code received</summary>
        public const string LastStatusCode = "LastStatusCode";

        /// <summary>Key for storing the last request elapsed time in milliseconds</summary>
        public const string LastElapsedMs = "LastElapsedMs";
        #endregion

        #region Schema Validation Keys
        /// <summary>Key for storing booking schema validation data</summary>
        public const string BookingSchema = "BookingSchema";

        /// <summary>Key for storing booking created response schema validation data</summary>
        public const string BookingCreatedResponseSchema = "BookingCreatedResponseSchema";

        /// <summary>Key for storing auth response schema validation data</summary>
        public const string AuthResponseSchema = "AuthResponseSchema";
        #endregion

        #region Deprecated Keys Warning
        // NOTE: Direct usage of these keys is discouraged in favor of typed extension methods
        // Example: Instead of _scenarioContext[ScenarioKeys.CurrentBooking] 
        //          Use: _scenarioContext.GetCurrentBooking()
        #endregion
    }
}