using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

public class JsonPropertyConverter : JsonConverter<Property>
{
    public override Property Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var key = reader.GetString();

        var segments = key?.Split(':') ?? [];
        if (segments.Length != 2)
            throw new FormatException($"Invalid Property format '{key}'");
        
        var origin = segments[0].ToType();
        var path = segments[1];

        return origin is not null ? Property.This(origin).GetProperty(path) : Property.Default;
    }

    public override void Write(Utf8JsonWriter writer, Property value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Key);
    }
}