using System.Text.Json;

namespace RestfulBookerTests.Helpers
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions _defaultOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Safely deserializes JSON content to a typed object and throws if invalid.
        /// </summary>
        public static T DeserializeSafe<T>(string? jsonContent, string errorMessage) where T : class
        {
            if (string.IsNullOrWhiteSpace(jsonContent))
                throw new InvalidOperationException($"{errorMessage} Response content is null or empty.");

            var trimmed = jsonContent.TrimStart();
            if (!(trimmed.StartsWith("{") || trimmed.StartsWith("[")))
                throw new InvalidOperationException($"{errorMessage} Expected JSON but got: {jsonContent}");

            var obj = JsonSerializer.Deserialize<T>(jsonContent, _defaultOptions);

            return obj ?? throw new InvalidOperationException($"{errorMessage} Could not parse JSON to the expected type {typeof(T).Name}.");
        }
    }
}
