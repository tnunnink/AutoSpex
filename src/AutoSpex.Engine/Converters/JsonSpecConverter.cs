using System.Text.Json;
using System.Text.Json.Serialization;
using AutoSpex.Engine.Migrations;

namespace AutoSpex.Engine;

public class JsonSpecConverter : JsonConverter<Spec>
{
    private static readonly Lazy<List<ISpecMigration>> Migrations = new(GetMigrations());

    public override Spec? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var element = JsonDocument.ParseValue(ref reader).RootElement;
        var spec = Migrate(element);
        return JsonSerializer.Deserialize<Spec>(spec);
    }

    public override void Write(Utf8JsonWriter writer, Spec value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }

    private static string Migrate(JsonElement element)
    {
        var version = element.TryGetProperty("SchemaVersion", out var versionProperty) ? versionProperty.GetInt32() : 0;

        if (version >= Migrations.Value.Last().Version)
            return element.GetRawText();

        var migrations = Migrations.Value.Where(m => m.Version > version).ToList();
        var migrated = migrations.Aggregate(element.GetRawText(), (json, migration) => migration.Run(json));
        return migrated;
    }

    private static List<ISpecMigration> GetMigrations()
    {
        var migrations = new List<ISpecMigration>();

        var types = typeof(JsonSpecConverter).Assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i == typeof(ISpecMigration)));

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);
            if (instance is not ISpecMigration migration)
                throw new InvalidOperationException($"Could not instantiate the ISpecMigration '{type.FullName}'");
            migrations.Add(migration);
        }

        return migrations.OrderBy(m => m.Version).ToList();
    }
}