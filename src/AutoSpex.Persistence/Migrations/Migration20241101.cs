using System.Data;
using System.Text;
using System.Text.Json;
using Dapper;
using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
//[Migration(20241101, "Transform spec configs to new schema")]
public class Migration20241101 : Migration
{
    public override void Up()
    {
        Execute.WithConnection(TransformSpecs);
    }

    public override void Down()
    {
        throw new NotSupportedException("Rolbacks not supported for this migration");
    }

    private static void TransformSpecs(IDbConnection connection, IDbTransaction transaction)
    {
        var specs = new Dictionary<string, string>();

        var records = connection.Query("SELECT SpecId, Config FROM Spec", transaction);
        
        foreach (var record in records)
        {
            var id = record.SpecId.ToString();
            var config = record.Config.ToString();
            var transformed = Transform(config);
            specs.TryAdd(id, transformed);
        }

        connection.Execute("UPDATE Spec SET Config = @Config WHERE SpecId = @SpecId",
            specs.Select(s => new { SpecId = s.Key, Config = s.Value }),
            transaction);
    }

    private static string Transform(string json)
    {
        var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteString("SpecId", root.GetProperty("SpecId").GetString());
        writer.WriteString("Element", root.GetProperty("Element").GetString());
        WriteTransformedCriterionArray(writer, "Filters", root);
        WriteTransformedCriterionArray(writer, "Verifications", root);
        writer.WriteNumber("FilterInclusion", root.GetProperty("FilterInclusion").GetInt32());
        writer.WriteNumber("VerificationInclusion", root.GetProperty("VerificationInclusion").GetInt32());
        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteTransformedCriterionArray(Utf8JsonWriter writer, string propertyName, JsonElement root)
    {
        writer.WriteStartArray(propertyName);

        foreach (var item in root.GetProperty(propertyName).EnumerateArray())
        {
            writer.WriteStartObject();
            writer.WriteString("Type", item.GetProperty("Type").GetString());
            writer.WritePropertyName("Property");
            item.GetProperty("Property").WriteTo(writer);
            writer.WriteString("Negation", item.GetProperty("Negation").GetString());
            writer.WriteString("Operation", item.GetProperty("Operation").GetString());
            writer.WritePropertyName("Argument");
            item.GetProperty("Argument").GetProperty("Value").WriteTo(writer);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
}