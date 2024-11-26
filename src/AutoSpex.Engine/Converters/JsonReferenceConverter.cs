using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

public class JsonReferenceConverter : JsonConverter<Reference>
{
    public override Reference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Reference.Parse(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, Reference value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}