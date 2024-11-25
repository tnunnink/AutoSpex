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

        writer.WritePropertyName("Steps");
        writer.WriteStartArray();

        foreach (var step in root.GetProperty("Steps").EnumerateArray())
            MigrateStep(writer, step);

        writer.WriteEndArray();

        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Migrates the step object. Only changes are to nested criterion property values.
    /// </summary>
    private static void MigrateStep(Utf8JsonWriter writer, JsonElement step)
    {
        writer.WriteStartObject();

        foreach (var property in step.EnumerateObject())
        {
            if (property.NameEquals("Criteria"))
            {
                writer.WritePropertyName("Criteria");
                writer.WriteStartArray();

                foreach (var criteria in property.Value.EnumerateArray())
                    MigrateCriterion(writer, criteria);

                writer.WriteEndArray();
            }
            else
            {
                writer.WritePropertyName(property.Name);
                property.Value.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
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

            if (property.NameEquals("Argument") && property.Value.GetProperty("Group").GetString() == "Criterion")
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