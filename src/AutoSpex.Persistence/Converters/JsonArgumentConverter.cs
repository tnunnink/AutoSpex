using System.Text.Json;
using System.Text.Json.Serialization;
using AutoSpex.Engine;

namespace AutoSpex.Persistence;

public class JsonArgumentConverter : JsonConverter<Argument>
{
    public override Argument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Type? type = default;
        object? value = default;

        while (reader.Read() & reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case nameof(Argument.Type):
                    type = reader.GetString()?.ToType();
                    break;
                case nameof(Argument.Value):
                    if (type is null)
                        throw new JsonException("Can not deserialize argument without a known type.");
                    value = JsonSerializer.Deserialize(ref reader, type, options);
                    break;
            }
        }

        if (value is null)
            throw new JsonException("Could not deserialize argument..");
        
        return new Argument(value);
    }

    public override void Write(Utf8JsonWriter writer, Argument value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(Argument.Type), value.Type.FullName);
        writer.WritePropertyName(nameof(Argument.Value));
        JsonSerializer.Serialize(writer, value.Value, value.Type, options);
        writer.WriteEndObject();
    }
}