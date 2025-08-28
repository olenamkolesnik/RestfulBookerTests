using Json.Schema;
using System;
using System.Linq;
using System.Text.Json;

namespace RestfulBookerTests.Helpers
{
    public static class SchemaValidationHelper
    {
        public static void ValidateAgainstSchema(object response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));

            var type = response.GetType();

            // Generate schema dynamically
            var schemaJson = SchemaGeneratorHelper.GenerateSchemaAsString(type);
            var schema = JsonSerializer.Deserialize<JsonSchema>(schemaJson)!;

            // Serialize response to JSON
            var json = JsonSerializer.Serialize(response);
            using var doc = JsonDocument.Parse(json);

            // Evaluate
            var result = schema.Evaluate(doc.RootElement);

            if (!result.IsValid)
            {
                var errors = string.Join("\n", result.Details
                    .Where(d => d.HasErrors)
                    .SelectMany(d => d.Errors!.Select(e => $"{d.InstanceLocation}: {e.Key} - {e.Value}")));

                throw new InvalidOperationException($"Schema validation failed for {type.Name}:\n{errors}");
            }
        }
    }
}
