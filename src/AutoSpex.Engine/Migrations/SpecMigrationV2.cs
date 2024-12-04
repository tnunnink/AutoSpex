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

        WriterQuery(writer, root);
        WriteVerifications(writer, root);

        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Write the new query object with the current element and single filter step.
    /// </summary>
    private static void WriterQuery(Utf8JsonWriter writer, JsonElement root)
    {
        writer.WritePropertyName("Query");
        writer.WriteStartObject();
        writer.WriteString("Element", root.GetProperty("Element").GetString());
        writer.WritePropertyName("Steps");
        writer.WriteStartArray();
        WriteFilter(writer, root);
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    /// <summary>
    /// Write the new filter object as a step in the query. 
    /// </summary>
    private static void WriteFilter(Utf8JsonWriter writer, JsonElement root)
    {
        writer.WriteStartObject();
        writer.WriteString("$type", "Filter");
        writer.WriteString("Match", "All");
        writer.WritePropertyName("Criteria");
        WriteCriteria(writer, root.GetProperty("Filters"));
        writer.WriteEndObject();
    }

    /// <summary>
    /// Write the verification criteria. 
    /// </summary>
    private static void WriteVerifications(Utf8JsonWriter writer, JsonElement root)
    {
        writer.WritePropertyName("Criteria");
        WriteCriteria(writer, root.GetProperty("Verifications"));
    }

    /// <summary>
    /// Migrates a collection of criterion
    /// </summary>
    private static void WriteCriteria(Utf8JsonWriter writer, JsonElement element)
    {
        writer.WriteStartArray();

        foreach (var criterion in element.EnumerateArray())
        {
            MigrateCriterion(writer, criterion);
        }

        writer.WriteEndArray();
    }

    /// <summary>
    /// Writes the criterion object. For each property value strip off the type info.
    /// </summary>
    private static void MigrateCriterion(Utf8JsonWriter writer, JsonElement criteria)
    {
        writer.WriteStartObject();

        foreach (var property in criteria.EnumerateObject())
        {
            if (property.NameEquals("Property"))
            {
                var value = property.Value.GetString() ?? string.Empty;
                var updated = value[(value.IndexOf(':') + 1)..];
                writer.WriteString("Property", updated);
                continue;
            }

            if (property.NameEquals("Argument") && 
                property.Value.ValueKind != JsonValueKind.Null && 
                property.Value.GetProperty("Group").GetString() == "Criterion")
            {
                var argument = property.Value;
                writer.WritePropertyName("Argument");
                writer.WriteStartObject();
                writer.WriteString("Group", argument.GetProperty("Group").GetString());
                writer.WritePropertyName("Data");
                MigrateCriterion(writer, argument.GetProperty("Data"));
                writer.WriteEndObject();
                continue;
            }

            writer.WritePropertyName(property.Name);
            property.Value.WriteTo(writer);
        }

        writer.WriteEndObject();
    }
}