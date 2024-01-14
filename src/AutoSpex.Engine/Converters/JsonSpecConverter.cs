using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoSpex.Engine.Converters;

public class JsonSpecConverter : JsonConverter<Spec>
{
    public override Spec Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var spec = new Spec();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case nameof(Spec.Element):
                    spec.Element = Element.FromName(reader.GetString());
                    break;
                case nameof(Spec.Settings):
                    var settings = JsonSerializer.Deserialize<SpecSettings>(ref reader, options);
                    spec.Settings = settings ?? SpecSettings.Default;
                    break;
                case nameof(Spec.Filters):
                    var filters = JsonSerializer.Deserialize<Criterion[]>(ref reader, options) ??
                                  Enumerable.Empty<Criterion>();
                    spec.Filters.AddRange(filters);
                    break;
                case nameof(Spec.Verifications):
                    var verifications = JsonSerializer.Deserialize<Criterion[]>(ref reader, options) ??
                                        Enumerable.Empty<Criterion>();
                    spec.Verifications.AddRange(verifications);
                    break;
            }
        }

        return spec;
    }

    public override void Write(Utf8JsonWriter writer, Spec value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        writer.WriteString(nameof(Spec.Element), value.Element.Name);
        
        writer.WritePropertyName(nameof(Spec.Settings));
        JsonSerializer.Serialize(writer, value.Settings, options);
        
        writer.WritePropertyName(nameof(Spec.Filters));
        JsonSerializer.Serialize(writer, value.Filters.ToArray(), options);
        
        writer.WritePropertyName(nameof(Spec.Verifications));
        JsonSerializer.Serialize(writer, value.Verifications.ToArray(), options);

        writer.WriteEndObject();
    }
}