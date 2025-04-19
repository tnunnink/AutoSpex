using System.Data;
using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20250401, "Replace reference table Variable table.")]
public class Migration20250401 : Migration
{
    public override void Up()
    {
        Delete.Table("Reference");

        Create.Table("Variable")
            .WithColumn("VariableId").AsString().PrimaryKey()
            .WithColumn("NodeId").AsString().Nullable().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Group").AsString().NotNullable()
            .WithColumn("Value").AsString().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Variable");
        
        Create.Table("Reference")
            .WithColumn("ReferenceId").AsInt64().PrimaryKey().Identity()
            .WithColumn("Scope").AsString().NotNullable().Unique()
            .WithColumn("Element").AsString().NotNullable()
            .WithColumn("Content").AsString().NotNullable();
    }
}