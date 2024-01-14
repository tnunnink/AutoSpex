using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoSpex.Engine.Converters;

public class JsonCriterionConverter : JsonConverter<Criterion>
{
    public override Criterion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var criterion = new Criterion();
        
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            reader.Read();
            
            switch (propertyName)
            {
                case nameof(Criterion.Property):
                    criterion.Property = reader.GetString();
                    break;
                case nameof(Criterion.Operation):
                    var operation = Operation.FromName(reader.GetString());
                    criterion.Operation = operation;
                    break;
                case nameof(Criterion.Arguments):
                    var args = JsonSerializer.Deserialize<Argument[]>(ref reader, options);
                    if (args is null) break;
                    criterion.Arguments = [..args];
                    break;
            }
        }

        return criterion;
    }

    public override void Write(Utf8JsonWriter writer, Criterion value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        if (value.Property != null)
        {
            writer.WriteString(nameof(Criterion.Property), value.Property);
        }

        writer.WriteString(nameof(Criterion.Operation), value.Operation.Name);

        writer.WritePropertyName(nameof(Criterion.Arguments));
        JsonSerializer.Serialize(writer, value.Arguments.ToArray(), options);

        writer.WriteEndObject();
    }
}