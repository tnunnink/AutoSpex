using System.Data;
using FluentMigrator;
using JetBrains.Annotations;


namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20241219, "Merge Suppression and Override into single contrusct/table called Action")]
public class Migration20241219 : Migration
{
    public override void Up()
    {
        Delete.Table("Suppression");
        Delete.Table("Override");

        Create.Table("Action")
            .WithColumn("SourceId").AsString().NotNullable().ForeignKey("Source", "SourceId").OnDelete(Rule.Cascade)
            .WithColumn("NodeId").AsString().NotNullable().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Type").AsString().NotNullable()
            .WithColumn("Reason").AsString().NotNullable()
            .WithColumn("Config").AsString().Nullable();

        Create.UniqueConstraint("UQ_Rule_SourceId_NodeId").OnTable("Action").Columns("SourceId", "NodeId");
    }

    public override void Down()
    {
        Delete.Table("Action");

        Create.Table("Suppression")
            .WithColumn("SourceId")
            .AsString().NotNullable().ForeignKey("Source", "SourceId")
            .OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("NodeId")
            .AsString().NotNullable().ForeignKey("Node", "NodeId")
            .OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("Reason")
            .AsString().NotNullable();

        Create.UniqueConstraint("UQ_Suppression_SourceId_NodeId").OnTable("Suppression").Columns("SourceId", "NodeId");

        Create.Table("Override")
            .WithColumn("SourceId")
            .AsString().NotNullable().ForeignKey("Source", "SourceId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("NodeId")
            .AsString().NotNullable().ForeignKey("Node", "NodeId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("Config")
            .AsString().NotNullable();

        Create.UniqueConstraint("UQ_Override_SourceId_NodeId").OnTable("Override").Columns("SourceId", "NodeId");
    }
}