using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20241227, "Update Run table to include Duration and PassRate")]
public class Migration20241227 : Migration
{
    public override void Up()
    {
        Delete.Table("Run");
        
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

    public override void Down()
    {
        Delete.Table("Run");
        
        Create.Table("Run")
            .WithColumn("RunId").AsString().PrimaryKey()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Node").AsString().NotNullable()
            .WithColumn("Source").AsString().NotNullable()
            .WithColumn("Result").AsString().NotNullable()
            .WithColumn("RanOn").AsString().NotNullable()
            .WithColumn("RanBy").AsString().NotNullable()
            .WithColumn("Outcomes").AsString().Nullable();
    }
}