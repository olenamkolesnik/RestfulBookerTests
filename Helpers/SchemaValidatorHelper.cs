using Json.Schema;
using System.Text.Json;

namespace RestfulBookerTests.Helpers
{
    public static class SchemaValidatorHelper
    {
        /// <summary>
        /// Validates a JSON string against a schema file.
        /// </summary>
        /// <param name="jsonResponse">The API response as a string.</param>
        /// <param name="schemaFilePath">Path to the schema file.</param>
        /// <returns>(bool IsValid, List&lt;string&gt; Errors)</returns>
        public static (bool IsValid, List<string> Errors) ValidateJsonAgainstSchema(string jsonResponse, string schemaFilePath)
        {
            if (string.IsNullOrWhiteSpace(jsonResponse))
                return (false, new List<string> { "Response is empty or null." });

            if (!File.Exists(schemaFilePath))
                return (false, new List<string> { $"Schema file not found: {schemaFilePath}" });

            try
            {
                // Load schema from file
                var schemaText = File.ReadAllText(schemaFilePath);
                var schema = JsonSerializer.Deserialize<JsonSchema>(schemaText);

                if (schema == null)
                    return (false, new List<string> { "Failed to parse schema file." });

                // Parse API response
                var jsonDocument = JsonDocument.Parse(jsonResponse);

                // Validate
                var result = schema.Evaluate(jsonDocument.RootElement);

                if (result.IsValid)
                    return (true, new List<string>());

                var errors = result.Errors?.Select(e => e.ToString()).ToList() ?? new List<string> { "Unknown validation error" };
                return (false, errors);
            }
            catch (Exception ex)
            {
                return (false, new List<string> { $"Validation error: {ex.Message}" });
            }
        }
    }
}
