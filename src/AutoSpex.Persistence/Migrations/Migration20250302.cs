using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20250302, "Delete Run table.")]
public class Migration20250302 : Migration
{
    public override void Up()
    {
        Delete.Table("Run");
    }

    public override void Down()
    {
        Create.Table("Run")
            .WithColumn("RunId").AsString().PrimaryKey()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Node").AsString().NotNullable()
            .WithColumn("Source").AsString().NotNullable()
            .WithColumn("Result").AsString().NotNullable()
            .WithColumn("RanOn").AsString().NotNullable()
            .WithColumn("RanBy").AsString().NotNullable()
            .WithColumn("Duration").AsInt64().NotNullable()
            .WithColumn("PassRate").AsDouble().NotNullable()
            .WithColumn("Outcomes").AsString().Nullable();
    }
}