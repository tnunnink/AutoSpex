using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20250327, "Add Run history table")]
public class Migration20250327 : Migration
{
    public override void Up()
    {
        Create.Table("Run")
            .WithColumn("RunId").AsString().PrimaryKey()
            .WithColumn("GroupId").AsString().NotNullable()
            .WithColumn("Node").AsString().NotNullable()
            .WithColumn("Source").AsString().NotNullable()
            .WithColumn("Location").AsString().NotNullable()
            .WithColumn("Result").AsString().NotNullable()
            .WithColumn("RanOn").AsString().NotNullable()
            .WithColumn("RanBy").AsString().NotNullable()
            .WithColumn("Duration").AsString().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Run");
    }
}