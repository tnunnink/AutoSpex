using System.Data;
using FluentMigrator;

namespace AutoSpex.Persistence;

[MigrationId(1, 00, 00, "Initial Project Build")]
[Tags("Project")]
public class MP10000 : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("Node")
            .WithColumn("NodeId").AsString().PrimaryKey()
            .WithColumn("ParentId").AsString().Nullable().ForeignKey("Node", "NodeId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("NodeType").AsString().NotNullable()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Depth").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("Ordinal").AsInt32().NotNullable().WithDefaultValue(0);

        Create.Table("Spec")
            .WithColumn("NodeId").AsString().PrimaryKey().ForeignKey("Node", "NodeId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("Specification").AsString().NotNullable();

        Create.Table("Source")
            .WithColumn("SourceId").AsString().PrimaryKey()
            .WithColumn("IsSelected").AsBoolean().NotNullable()
            .WithColumn("Name").AsString().NotNullable().Unique()
            .WithColumn("Description").AsString().Nullable()
            .WithColumn("TargetType").AsString().Nullable()
            .WithColumn("TargetName").AsString().Nullable()
            .WithColumn("ExportedBy").AsString().Nullable()
            .WithColumn("ExportedOn").AsDate().Nullable()
            .WithColumn("Content").AsString().NotNullable();

        Create.Table("Variable")
            .WithColumn("VariableId").AsString().PrimaryKey()
            .WithColumn("NodeId").AsString().NotNullable().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Value").AsString().NotNullable();

        Create.UniqueConstraint("Unique_Variable_NodeId_Name")
            .OnTable("Variable")
            .Columns("NodeId", "Name");

        Create.Table("Runner")
            .WithColumn("RunnerId").AsString().PrimaryKey()
            .WithColumn("SourceId").AsString().Nullable().ForeignKey("Source", "SourceId").OnDelete(Rule.SetNull)
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Result").AsString().Nullable()
            .WithColumn("RanOn").AsString().Nullable();

        Create.Table("Outcome")
            .WithColumn("OutcomeId").AsString().PrimaryKey()
            .WithColumn("RunnerId").AsString().NotNullable().ForeignKey("Runner", "RunnerId").OnDelete(Rule.Cascade)
            .WithColumn("NodeId").AsString().NotNullable().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Result").AsString().NotNullable()
            .WithColumn("SpecName").AsString().NotNullable()
            .WithColumn("SpecPath").AsString().NotNullable()
            .WithColumn("Duration").AsInt32().NotNullable()
            .WithColumn("Verified").AsInt32().NotNullable()
            .WithColumn("Passed").AsInt32().NotNullable()
            .WithColumn("Failed").AsInt32().NotNullable()
            .WithColumn("Errored").AsInt32().NotNullable()
            .WithColumn("Verifications").AsString();
        
        Create.Table("Override")
            .WithColumn("RunnerId").AsString().NotNullable()
            .ForeignKey("Runner", "RunnerId").OnDelete(Rule.Cascade)
            .WithColumn("VariableId").AsString().NotNullable()
            .ForeignKey("Variable", "VariableId").OnDelete(Rule.Cascade)
            .WithColumn("Value").AsString().NotNullable();

        Create.UniqueConstraint("Unique_Override_Runner_Variable")
            .OnTable("Override")
            .Columns("RunnerId", "VariableId");

        Create.Table("ChangeLog")
            .WithColumn("ChangeId").AsString().PrimaryKey()
            .WithColumn("NodeId").AsString().NotNullable().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("ChangeType").AsString().NotNullable()
            .WithColumn("ChangedOn").AsString().NotNullable()
            .WithColumn("ChangedBy").AsString().NotNullable();

        Create.Table("Documentation")
            .WithColumn("OwnerId").AsString().PrimaryKey()
            .WithColumn("Text").AsString();
    }
}