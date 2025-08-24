using Json.Schema;
using Json.Schema.Generation;
using System;
using System.Text.Json;

public static class SchemaGeneratorHelper
{
    public static string GenerateSchemaAsString(Type modelType)
    {
        var builder = new JsonSchemaBuilder().FromType(modelType);
        var schema = builder.Build();

        // Serialize schema to JSON (pretty print)
        var schemaJson = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        return schemaJson;
    }

    public static string GenerateSchemaAsString<T>()
    {
        return GenerateSchemaAsString(typeof(T));
    }
}
