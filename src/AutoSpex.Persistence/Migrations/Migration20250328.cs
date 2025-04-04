using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20250328, "Added default settings.")]
public class Migration20250328 : Migration
{
    public override void Up()
    {
        Insert.IntoTable("Setting")
            .Row(new { Key = "Theme", Value = "Light" })
            .Row(new { Key = "ShellHeight", Value = 800 })
            .Row(new { Key = "ShellWidth", Value = 1400 })
            .Row(new { Key = "ShellState", Value = "Normal" })
            .Row(new { Key = "AlwaysDiscardChanges", Value = false });
    }

    public override void Down()
    {
        Delete.FromTable("Setting")
            .Row(new { Key = "Theme" })
            .Row(new { Key = "ShellHeight" })
            .Row(new { Key = "ShellWidth" })
            .Row(new { Key = "ShellState" })
            .Row(new { Key = "AlwaysDiscardChanges" });
    }
}