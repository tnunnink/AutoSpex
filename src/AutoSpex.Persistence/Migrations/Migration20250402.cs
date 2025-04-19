using System.Data;
using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
//[Migration(20250402, "Add Source, Variable, Reference tables.")]
public class Migration20250402 : Migration
{
    public override void Up()
    {
        Create.Table("Profile")
            .WithColumn("ProfileId").AsString().PrimaryKey()
            .WithColumn("NodeId").AsString().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Target").AsString().NotNullable()
            .WithColumn("Condition").AsString().NotNullable();

        Create.Table("Rule")
            .WithColumn("RuleId").AsString().PrimaryKey()
            .WithColumn("ProfileId").AsString().ForeignKey("Profile", "ProfileId").OnDelete(Rule.Cascade)
            .WithColumn("Type").AsString().NotNullable()
            .WithColumn("Config").AsString().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Profile");
        Delete.Table("Rule");
    }
}