using System.Data;
using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20250215, "Delete Action table.")]
public class Migration20250215 : Migration
{
    public override void Up()
    {
        Delete.Table("Action");
    }

    public override void Down()
    {
        Create.Table("Action")
            .WithColumn("SourceId").AsString().NotNullable().ForeignKey("Source", "SourceId").OnDelete(Rule.Cascade)
            .WithColumn("NodeId").AsString().NotNullable().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Type").AsString().NotNullable()
            .WithColumn("Reason").AsString().NotNullable()
            .WithColumn("Config").AsString().Nullable();

        Create.UniqueConstraint("UQ_Rule_SourceId_NodeId").OnTable("Action").Columns("SourceId", "NodeId");
    }
}