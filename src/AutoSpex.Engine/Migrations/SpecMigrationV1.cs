using System.Text;
using System.Text.Json;
using JetBrains.Annotations;

namespace AutoSpex.Engine.Migrations;

[UsedImplicitly]
public class SpecMigrationV1 : ISpecMigration
{
    /// <inheritdoc />
    public int Version => 1;

    /// <inheritdoc />
    public string Run(string json)
    {
        var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteNumber("SchemaVersion", 1);
        writer.WriteString("SpecId", root.GetProperty("SpecId").GetString());
        writer.WriteString("Element", root.GetProperty("Element").GetString());
        MigrateCriteria(writer, "Filters", root);
        MigrateCriteria(writer, "Verifications", root);
        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Migrate each criterion object in the specified collection (Filters/Verification) 
    /// </summary>
    private static void MigrateCriteria(Utf8JsonWriter writer, string propertyName, JsonElement root)
    {
        writer.WriteStartArray(propertyName);

        foreach (var item in root.GetProperty(propertyName).EnumerateArray())
        {
            MigrateCriterion(writer, item);
        }

        writer.WriteEndArray();
    }

    /// <summary>
    /// Migrates a single criterion object to the new schema.
    /// </summary>
    private static void MigrateCriterion(Utf8JsonWriter writer, JsonElement item)
    {
        writer.WriteStartObject();
        writer.WriteString("Type", item.GetProperty("Type").GetString());
        MigrateProperty(writer, item);
        writer.WriteString("Negation", item.GetProperty("Negation").GetString());
        var isArgumentWritten = MigrateOperation(writer, item);
        if (!isArgumentWritten) MigrateArgument(writer, item);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Convert the property to a single string value Type:Path
    /// </summary>
    private static void MigrateProperty(Utf8JsonWriter writer, JsonElement item)
    {
        var origin = item.Get("Property")?.Get("Origin")?.GetString();
        var path = item.Get("Property")?.Get("Path")?.GetString();
        var property = string.Concat(origin, ':', path);
        writer.WriteString("Property", property);
    }

    /// <summary>
    /// If the operation is the True/False operation we need to replace that with equal to false/true since they
    /// were removed. We have to return a flag indicating whether we wrote the arugment so to avoind the next
    /// migration step for argument.
    /// </summary>
    private static bool MigrateOperation(Utf8JsonWriter writer, JsonElement item)
    {
        var operation = item.GetProperty("Operation").GetString();

        switch (operation)
        {
            case "False":
                writer.WriteString("Operation", Operation.EqualTo.Name);
                writer.WritePropertyName("Argument");
                TypeGroup.Boolean.WriteData(writer, false);
                return true;
            case "True":
                writer.WriteString("Operation", Operation.EqualTo.Name);
                writer.WritePropertyName("Argument");
                TypeGroup.Boolean.WriteData(writer, true);
                return true;
            default:
                writer.WriteString("Operation", operation);
                return false;
        }
    }

    /// <summary>
    /// Flatten the argument object structure and ensure we use the new corresponding TypeGroup to serialize the data. 
    /// </summary>
    private static void MigrateArgument(Utf8JsonWriter writer, JsonElement item)
    {
        writer.WritePropertyName("Argument");

        var type = item.Get("Argument")?.Get("Value")?.Get("Type")?.GetString().ToType();
        var data = item.Get("Argument")?.Get("Value")?.Get("Data")?.GetString();

        var group = TypeGroup.FromType(type);

        //We need to migrate any inner criterion object without deserializing it first.
        if (group == TypeGroup.Criterion && !string.IsNullOrEmpty(data))
        {
            var json = JsonDocument.Parse(data);
            var element = json.RootElement;

            writer.WriteStartObject();
            writer.WriteString("Group", TypeGroup.Criterion.Name);
            writer.WritePropertyName("Data");
            MigrateCriterion(writer, element);
            writer.WriteEndObject();
            return;
        }

        if (string.IsNullOrEmpty(data) || !group.TryParse(data, out var parsed))
        {
            TypeGroup.Default.WriteData(writer, null);
            return;
        }

        group.WriteData(writer, parsed);
    }
}