using System.Text.Json;

namespace RestfulBookerTests.Helpers
{
    public static class JsonHelper
    {
        /// <summary>
        /// Safely deserializes JSON content to a typed object and throws if null or empty.
        /// </summary>
        public static T DeserializeSafe<T>(string? jsonContent, string errorMessage) where T : class
        {
            if (string.IsNullOrWhiteSpace(jsonContent))
                throw new InvalidOperationException(errorMessage + " Response content is null or empty.");
            if (!jsonContent.TrimStart().StartsWith("{"))
                throw new InvalidOperationException($"Expected JSON but got: {jsonContent}");

            var obj = JsonSerializer.Deserialize<T>(jsonContent);

            return obj ?? throw new InvalidOperationException(errorMessage + " Could not parse JSON to the expected type.");
        }
    }
}
