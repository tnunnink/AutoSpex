using System.Data;
using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20241024, "Adds Ignore Table")]
public class Migration20241024 : Migration
{
    public override void Up()
    {
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
    }

    public override void Down()
    {
        Delete.UniqueConstraint("UQ_Suppression_SourceId_NodeId").FromTable("Suppression");
        Delete.Table("Suppression");
    }
}