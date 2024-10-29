using System.Data;
using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20241027, "Override Table: Update SpecId to NodeId")]
public class Migration20241027 : Migration
{
    public override void Up()
    {
        Delete.Table("Override");

        Create.Table("Override")
            .WithColumn("SourceId")
            .AsString().NotNullable().ForeignKey("Source", "SourceId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("NodeId")
            .AsString().NotNullable().ForeignKey("Node", "NodeId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("Config")
            .AsString().NotNullable();

        Create.UniqueConstraint("UQ_Override_SourceId_NodeId").OnTable("Override").Columns("SourceId", "NodeId");
    }

    public override void Down()
    {
        Delete.Table("Override");

        Create.Table("Override")
            .WithColumn("SourceId")
            .AsString().NotNullable().ForeignKey("Source", "SourceId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("SpecId")
            .AsString().NotNullable().ForeignKey("Spec", "SpecId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("Config")
            .AsString().NotNullable();

        Create.UniqueConstraint("UQ_Override_SourceId_SpecId").OnTable("Override").Columns("SourceId", "SpecId");
    }
}