using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20250327, "Create Setting table")]
public class Migration20250327 : Migration
{
    public override void Up()
    {
        Create.Table("Setting")
            .WithColumn("Key").AsString().PrimaryKey()
            .WithColumn("Value").AsString().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Setting");
    }
}