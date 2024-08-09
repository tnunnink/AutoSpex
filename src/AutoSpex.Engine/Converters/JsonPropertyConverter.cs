using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

public class JsonPropertyConverter : JsonConverter<Property>
{
    public override Property? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var origin = typeof(object);
        var path = string.Empty;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case nameof(Property.Origin):
                    origin = reader.GetString().ToType();
                    break;
                case nameof(Property.Path):
                    path = reader.GetString();
                    break;
            }
        }

        return origin is not null ? Property.This(origin).GetProperty(path) : Property.Default;
    }

    public override void Write(Utf8JsonWriter writer, Property value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(Property.Origin), value.Origin.FullName);
        writer.WriteString(nameof(Property.Path), value.Path);
        writer.WriteEndObject();
    }
}