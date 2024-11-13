using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

public class JsonObjectConverter : JsonConverter<object?>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var group = TypeGroup.Default;
        object? value = default;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "Group":
                    group = TypeGroup.FromName(reader.GetString());
                    break;
                case "Data":
                    value = group.ReadData(ref reader, options);
                    break;
            }
        }

        return value;
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        var group = TypeGroup.FromType(value?.GetType());
        group.WriteData(writer, value, options);
    }
}