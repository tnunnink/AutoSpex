using System.Text.Json;
using System.Text.Json.Serialization;
using L5Sharp.Core;

namespace AutoSpex.Engine;

public class JsonArgumentConverter : JsonConverter<Argument>
{
    public override Argument? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var id = Guid.Empty;
        var type = typeof(object);
        object? value = default;

        while (reader.Read() & reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case nameof(Argument.ArgumentId):
                    id = JsonSerializer.Deserialize<Guid>(ref reader, options);
                    break;
                case nameof(Argument.Type):
                    type = JsonSerializer.Deserialize<Type>(ref reader, options);
                    break;
                case nameof(Argument.Value):
                    if (type is null) break;

                    if (type == typeof(Variable) || type == typeof(Criterion))
                    {
                        //These are the only supported complex types argument should contain.
                        value = JsonSerializer.Deserialize(ref reader, type, options);
                    }
                    else
                    {
                        //Everything else should be text which we will try to parse using L5Sharp.
                        var text = reader.GetString();
                        value = text?.TryParse(type) ?? text;
                    }

                    break;
            }
        }

        return new Argument(id, value);
    }

    public override void Write(Utf8JsonWriter writer, Argument value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(Argument.ArgumentId), value.ArgumentId);

        if (value.Type is not null)
            writer.WriteString(nameof(Argument.Type), value.Type.FullName);

        if (value.Value is null)
        {
            writer.WriteEndObject();
            return;
        }

        if (value.Type == typeof(Variable) || value.Type == typeof(Criterion))
        {
            writer.WritePropertyName(nameof(Argument.Value));
            JsonSerializer.Serialize(writer, value.Value, value.Type, options);
        }
        else
        {
            writer.WriteString(nameof(Argument.Value), value.Value.ToString());
        }

        writer.WriteEndObject();
    }
}