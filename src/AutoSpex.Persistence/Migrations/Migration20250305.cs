using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20250305, "Add reference table.")]
public class Migration20250305 : Migration
{
    public override void Up()
    {
        Create.Table("Reference")
            .WithColumn("Scope").AsString().NotNullable().Unique();
    }

    public override void Down()
    {
        Delete.Table("Reference");
    }
}