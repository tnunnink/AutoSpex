using System.Text;
using System.Text.Json;
using JetBrains.Annotations;

namespace AutoSpex.Engine.Migrations;

[UsedImplicitly]
public class SpecMigrationV3 : ISpecMigration
{
    /// <inheritdoc />
    public int Version => 3;

    /// <inheritdoc />
    public string Run(string json)
    {
        var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteNumber("SchemaVersion", Version);
        writer.WriteString("SpecId", root.GetProperty("SpecId").GetString());
        writer.WriteString("Element", root.Get("Query")?.GetProperty("Element").GetString());
        WriterSteps(writer, root);
        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Write the new query object with the current element and a single filter step.
    /// </summary>
    private static void WriterSteps(Utf8JsonWriter writer, JsonElement root)
    {
        writer.WritePropertyName("Steps");
        writer.WriteStartArray();
        WriteQuerySteps(writer, root.GetProperty("Query"));
        WriteVerifyStep(writer, root.GetProperty("Verify"));
        writer.WriteEndArray();
    }

    /// <summary>
    /// Writes all current steps in the element to the current writer.
    /// </summary>
    private static void WriteQuerySteps(Utf8JsonWriter writer, JsonElement element)
    {
        foreach (var item in element.GetProperty("Steps").EnumerateArray())
        {
            writer.WriteRawValue(item.GetRawText());
        }
    }

    /// <summary>
    /// Write the verify step as part of the spec steps array. 
    /// </summary>
    private static void WriteVerifyStep(Utf8JsonWriter writer, JsonElement root)
    {
        writer.WriteStartObject();
        writer.WriteString("$type", "Verify");
        writer.WritePropertyName("Criteria");
        writer.WriteRawValue(root.GetProperty("Criteria").GetRawText());
        writer.WriteEndObject();
    }
}