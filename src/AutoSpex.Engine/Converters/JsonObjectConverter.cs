using System.Text.Json;
using System.Text.Json.Serialization;
using L5Sharp.Core;

namespace AutoSpex.Engine;

public class JsonObjectConverter : JsonConverter<object?>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var type = typeof(object);
        var data = string.Empty;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "Type":
                    type = reader.GetString().ToType();
                    break;
                case "Data":
                    data = reader.GetString();
                    break;
            }
        }

        if (type is null || data is null) return default;

        //Handle any known complex type that was serialized as JSON.
        if (type == typeof(Criterion)) return JsonSerializer.Deserialize<Criterion>(data);
        if (type == typeof(Reference)) return JsonSerializer.Deserialize<Reference>(data);
        if (type == typeof(Variable)) return JsonSerializer.Deserialize<Variable>(data);

        //LogixParser can handle all the other types we care about (.NET primitive and L5Sharp).
        return type.IsParsable() ? data.TryParse(type) : default;
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var type = value?.GetType() ?? typeof(object);

        var data = value switch
        {
            null => null,
            string v => v,
            LogixEnum v => v.Name,
            AtomicData v => v.ToString(),
            LogixElement v => v.Serialize().ToString(),
            Criterion v => JsonSerializer.Serialize(v),
            Reference v => JsonSerializer.Serialize(v),
            Variable v => JsonSerializer.Serialize(v),
            _ => value.ToString()
        };

        writer.WriteString("Type", type.FullName);
        writer.WriteString("Data", data);

        writer.WriteEndObject();
    }
}