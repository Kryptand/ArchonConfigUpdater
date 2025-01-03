using System.Text.Json;
using System.Text.Json.Serialization;
using ArchonConfigUpdater.Models;

namespace ArchonConfigUpdater;

public class AvailableClassesConverter : JsonConverter<AvailableClasses>
{
    public override AvailableClasses Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var className = reader.GetString();

        // Try to parse the string to enum and return the value if successful
        if (Enum.TryParse(className, true, out AvailableClasses result))
        {
            return result;
        }

        // If the string does not match, throw an exception
        throw new JsonException($"Unknown class name: {className}");
    }

    public override void Write(Utf8JsonWriter writer, AvailableClasses value, JsonSerializerOptions options)
    {
        // Convert the enum back to string for serialization
        writer.WriteStringValue(value.ToString());
    }
}