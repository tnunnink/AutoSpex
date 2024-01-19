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
            .WithColumn("NodeId").AsString().NotNullable().ForeignKey("Node", "NodeId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Value").AsString().NotNullable()
            .WithColumn("Description").AsString().Nullable();

        Create.UniqueConstraint("Unique_Variable_NodeId_Name")
            .OnTable("Variable")
            .Columns("NodeId", "Name");

        Create.Table("Runner")
            .WithColumn("RunnerId").AsString().PrimaryKey()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Description").AsString();

        Create.Table("RunnerNode")
            .WithColumn("RunnerId")
            .AsString().NotNullable()
            .ForeignKey("Runner", "RunnerId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("NodeId")
            .AsString().NotNullable()
            .ForeignKey("Node", "NodeId").OnDeleteOrUpdate(Rule.Cascade);

        Create.UniqueConstraint("Unique_RunnerSpec_Runner_Node")
            .OnTable("RunnerNode")
            .Columns("RunnerId", "NodeId");
        
        Create.Table("Override")
            .WithColumn("RunnerId").AsString().NotNullable()
            .ForeignKey("Runner", "RunnerId").OnDelete(Rule.Cascade)
            .WithColumn("VariableId").AsString().NotNullable()
            .ForeignKey("Variable", "VariableId").OnDelete(Rule.Cascade)
            .WithColumn("Value").AsString().NotNullable();
        
        Create.UniqueConstraint("Unique_Override_Runner_Variable")
            .OnTable("Override")
            .Columns("RunnerId", "VariableId");

        Create.Table("Run")
            .WithColumn("RunId").AsString().PrimaryKey()
            .WithColumn("RunnerId").AsString().ForeignKey("Runner", "RunnerId").OnDelete(Rule.SetNull)
            .WithColumn("SourceId").AsString().ForeignKey("Source", "SourceId").OnDelete(Rule.SetNull)
            .WithColumn("Result").AsString().NotNullable()
            .WithColumn("Ran").AsString().NotNullable()
            .WithColumn("Runner").AsString().NotNullable()
            .WithColumn("Source").AsString().NotNullable();

        Create.Table("Outcome")
            .WithColumn("OutcomeId").AsString().PrimaryKey()
            .WithColumn("NodeId").AsString().Nullable().ForeignKey("Node", "NodeId").OnDelete(Rule.SetNull)
            .WithColumn("RunId").AsString().NotNullable().ForeignKey("Run", "RunId").OnDelete(Rule.Cascade)
            .WithColumn("Result").AsString().NotNullable()
            .WithColumn("Spec").AsString().NotNullable()
            .WithColumn("Path").AsString().NotNullable()
            .WithColumn("ProducedOn").AsDateTime().NotNullable().WithDefaultValue(DateTime.Now)
            .WithColumn("Duration").AsInt32().NotNullable()
            .WithColumn("Verified").AsInt32().NotNullable()
            .WithColumn("Passed").AsInt32().NotNullable()
            .WithColumn("Failed").AsInt32().NotNullable()
            .WithColumn("Errored").AsInt32().NotNullable()
            .WithColumn("Verifications").AsString();

    }
}