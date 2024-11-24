using System.Text;
using System.Text.Json;
using JetBrains.Annotations;

namespace AutoSpex.Engine.Migrations;

[UsedImplicitly]
public class SpecMigrationV2 : ISpecMigration
{
    /// <inheritdoc />
    public int Version => 2;

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

        writer.WritePropertyName("Steps");
        writer.WriteStartArray();
        WriteQueryElement(writer, root);
        WriteFilterElement(writer, root);
        WriteVerifyElement(writer, root);
        writer.WriteEndArray();

        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Migrates the Element property into a Query Step object in the new steps array. 
    /// </summary>
    private static void WriteQueryElement(Utf8JsonWriter writer, JsonElement root)
    {
        writer.WriteStartObject();
        writer.WriteString("$type", "Query");
        writer.WriteString("Element", root.GetProperty("Element").GetString());
        writer.WritePropertyName("Criteria");
        writer.WriteStartArray();
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    /// <summary>
    /// Migrates the Filter criterion array into a Filter Step object in the new steps array. 
    /// </summary>
    private static void WriteFilterElement(Utf8JsonWriter writer, JsonElement root)
    {
        writer.WriteStartObject();
        writer.WriteString("$type", "Filter");
        writer.WriteString("Match", "All");
        writer.WritePropertyName("Criteria");
        writer.WriteRawValue(root.GetProperty("Filters").GetRawText());
        writer.WriteEndObject();
    }

    /// <summary>
    /// Migrates the Verifications criterion array into a Filter Step object in the new steps array. 
    /// </summary>
    private static void WriteVerifyElement(Utf8JsonWriter writer, JsonElement root)
    {
        writer.WriteStartObject();
        writer.WriteString("$type", "Verify");
        writer.WritePropertyName("Criteria");
        writer.WriteRawValue(root.GetProperty("Verifications").GetRawText());
        writer.WriteEndObject();
    }
}