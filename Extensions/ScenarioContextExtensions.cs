using Reqnroll;
using System.Diagnostics.CodeAnalysis;

namespace RestfulBookerTests.Extensions
{
    /// <summary>
    /// Extension methods for ScenarioContext to store and retrieve test data and clients.
    /// Provides both mandatory accessors (Get*) and safe optional accessors (TryGet*).
    /// </summary>
    public static class ScenarioContextExtensions
    {
        #region Client Management

        /// <summary>
        /// Gets a client of type T from the ScenarioContext.
        /// Throws InvalidOperationException if not found.
        /// Use this when a client is required for your tests.
        /// </summary>
        public static T GetClient<T>(this ScenarioContext context) where T : class
        {
            var key = typeof(T).Name;
            if (!context.TryGetValue<T>(key, out var client) || client == null)
            {
                throw new InvalidOperationException(
                    $"{typeof(T).Name} not found in ScenarioContext. Ensure it is initialized in TestHooks.");
            }
            return client;
        }

        /// <summary>
        /// Stores a client of type T in the ScenarioContext.
        /// </summary>
        public static void SetClient<T>(this ScenarioContext context, T client) where T : class
        {
            var key = typeof(T).Name;
            context[key] = client;
        }

        #endregion

        #region Mandatory Data Management

        /// <summary>
        /// Gets data of type T from the ScenarioContext.
        /// Throws InvalidOperationException if the key does not exist or the value is null (for reference types).
        /// Use this for required test data.
        /// </summary>
        public static T GetData<T>(this ScenarioContext context, string key)
        {
            if (!context.TryGetValue<T>(key, out var data) || (typeof(T).IsClass && data == null))
            {
                throw new InvalidOperationException(
                    $"Data with key '{key}' not found in ScenarioContext.");
            }
            return data;
        }

        /// <summary>
        /// Stores data of type T in the ScenarioContext under the given key.
        /// </summary>
        public static void SetData<T>(this ScenarioContext context, string key, T data)
        {
            context[key] = data!;
        }

        #endregion

        #region Optional / Safe Data Access

        /// <summary>
        /// Tries to get data of type T from the ScenarioContext.
        /// Returns true if the key exists and the value is non-null (for reference types).
        /// Returns false if the key does not exist or value is null (for reference types).
        /// Value types are returned directly; default(T) is returned if missing.
        /// </summary>
        /// <typeparam name="T">Type of data to retrieve.</typeparam>
        /// <param name="context">ScenarioContext instance.</param>
        /// <param name="key">Key under which the data is stored.</param>
        /// <param name="value">Output value; valid if method returns true.</param>
        /// <returns>True if data exists and is valid; otherwise false.</returns>
        public static bool TryGetData<T>(this ScenarioContext context, string key, [MaybeNullWhen(false)] out T value)
        {
            if (context.TryGetValue(key, out var obj) && obj is T typed)
            {
                // For reference types, fail if the stored value is null
                if (typeof(T).IsClass && typed == null)
                {
                    value = default;
                    return false;
                }

                value = typed;
                return true;
            }

            value = default;
            return false;
        }

        #endregion

        #region Logger Management

        /// <summary>
        /// Stores a logger instance in the ScenarioContext.
        /// </summary>
        public static void SetLogger(this ScenarioContext context, Microsoft.Extensions.Logging.ILogger logger)
        {
            context["Logger"] = logger;
        }

        /// <summary>
        /// Gets the logger from the ScenarioContext.
        /// Throws if the logger is not found.
        /// </summary>
        public static Microsoft.Extensions.Logging.ILogger GetLogger(this ScenarioContext context)
        {
            if (!context.TryGetValue<Microsoft.Extensions.Logging.ILogger>("Logger", out var logger) || logger == null)
            {
                throw new InvalidOperationException("Logger not found in ScenarioContext.");
            }
            return logger;
        }

        #endregion
    }
}
