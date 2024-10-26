using System.Data;
using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20241025, "Remove Variable. Replace Override")]
public class Migration20241025 : Migration
{
    public override void Up()
    {
        Delete.Table("Override");
        Delete.Table("Variable");
        
        Create.Table("Override")
            .WithColumn("SourceId")
            .AsString().NotNullable().ForeignKey("Source", "SourceId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("SpecId")
            .AsString().NotNullable().ForeignKey("Spec", "SpecId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("Config")
            .AsString().NotNullable();
        
        Create.UniqueConstraint("UQ_Override_SourceId_SpecId").OnTable("Override").Columns("SourceId", "SpecId");
    }

    public override void Down()
    {
        Delete.UniqueConstraint("UQ_Override_SourceId_SpecId").FromTable("Override");
        Delete.Table("Override");
        
        Create.Table("Variable")
            .WithColumn("VariableId").AsString().PrimaryKey()
            .WithColumn("NodeId").AsString().NotNullable().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Group").AsString().NotNullable()
            .WithColumn("Value").AsString().Nullable();

        Create.Table("Override")
            .WithColumn("OverrideId").AsString().PrimaryKey()
            .WithColumn("SourceId").AsString().ForeignKey("Source", "SourceId").OnDelete(Rule.Cascade)
            .WithColumn("VariableId").AsString().ForeignKey("Variable", "VariableId").OnDelete(Rule.Cascade)
            .WithColumn("Value").AsString().NotNullable();
    }
}