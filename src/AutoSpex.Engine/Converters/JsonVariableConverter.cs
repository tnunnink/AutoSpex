using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

public class JsonVariableConverter : JsonConverter<Variable>
{
    public override Variable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var name = string.Empty;
        var value = string.Empty;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case nameof(Variable.Name):
                    name = reader.GetString() ??
                           throw new JsonException("Can not convert Variable without Name property");
                    break;
                case nameof(Variable.Value):
                    value = reader.GetString() ??
                            throw new JsonException("Can not convert Variable without Value property");
                    break;
            }
        }

        return new Variable(name, value);
    }

    public override void Write(Utf8JsonWriter writer, Variable value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(Variable.Name), value.Name);
        writer.WriteString(nameof(Variable.Type), value.Type.FullName);
        
        //We will either serialize an inner variable or just write the string representation of the value.
        if (value.Type == typeof(Variable))
        {
            writer.WritePropertyName(nameof(Variable.Value));
            JsonSerializer.Serialize(writer, value.Value, value.Type, options);
        }
        else
        {
            writer.WriteString(nameof(Variable.Value), value.Value.ToString());
        }
        
        writer.WriteEndObject();
    }
}