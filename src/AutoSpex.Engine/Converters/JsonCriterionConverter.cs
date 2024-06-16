using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

public class JsonCriterionConverter : JsonConverter<Criterion>
{
    public override Criterion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var id = Guid.Empty;
        var type = typeof(object);
        var property = Property.This(type);
        var operation = Operation.None;
        var arguments = new List<Argument>();
        var invert = false;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case nameof(Criterion.CriterionId):
                    id = JsonSerializer.Deserialize<Guid>(ref reader, options);
                    break;
                case nameof(Criterion.Type):
                    type = JsonSerializer.Deserialize<Type>(ref reader, options);
                    break;
                case nameof(Criterion.Property):
                    if (type is null) break;
                    var path = reader.GetString();
                    property = Property.This(type).Descendant(path);
                    break;
                case nameof(Criterion.Operation):
                    operation = Operation.FromName(reader.GetString());
                    break;
                case nameof(Criterion.Arguments):
                    var args = JsonSerializer.Deserialize<Argument[]>(ref reader, options);
                    if (args is null) break;
                    arguments = [..args];
                    break;
                case nameof(Criterion.Invert):
                    invert = reader.GetBoolean();
                    break;
            }
        }

        // ReSharper disable once UseObjectOrCollectionInitializer
        return new Criterion(id, type, property, operation, invert, arguments.ToArray());
    }

    public override void Write(Utf8JsonWriter writer, Criterion value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(Criterion.CriterionId), value.CriterionId);

        if (value.Type is not null)
            writer.WriteString(nameof(Criterion.Type), value.Type.FullName);

        if (value.Property is not null)
            writer.WriteString(nameof(Criterion.Property), value.Property.Path);

        if (value.Operation is not null)
            writer.WriteString(nameof(Criterion.Operation), value.Operation.Name);

        writer.WritePropertyName(nameof(Criterion.Arguments));
        JsonSerializer.Serialize(writer, value.Arguments.ToArray(), options);

        writer.WriteBoolean(nameof(Criterion.Invert), value.Invert);

        writer.WriteEndObject();
    }
}