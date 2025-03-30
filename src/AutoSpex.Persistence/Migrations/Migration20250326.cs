using System.Data;
using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20250326, "Update reference table.")]
public class Migration20250326 : Migration
{
    public override void Up()
    {
        Delete.Table("Reference");

        Create.Table("Reference")
            .WithColumn("Scope").AsString().PrimaryKey()
            .WithColumn("NodeId").AsString().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Element").AsString().NotNullable()
            .WithColumn("Hash").AsString().NotNullable()
            .WithColumn("Content").AsString().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Reference");

        Create.Table("Reference")
            .WithColumn("Scope").AsString().NotNullable().Unique();
    }
}