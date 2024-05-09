using System.Data;
using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
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
            .WithColumn("Documentation").AsString().Nullable();

        Create.Table("Spec")
            .WithColumn("SpecId").AsString().PrimaryKey().ForeignKey("Node", "NodeId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("Element").AsString().NotNullable()
            .WithColumn("Specification").AsString().NotNullable();
        
        Create.Table("Variable")
            .WithColumn("VariableId").AsString().PrimaryKey()
            .WithColumn("NodeId").AsString().NotNullable().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Type").AsString().NotNullable()
            .WithColumn("Data").AsString().NotNullable()
            .WithColumn("Description").AsString().Nullable();
        
        Create.UniqueConstraint("Unique_Variable_NodeId_Name")
            .OnTable("Variable")
            .Columns("NodeId", "Name");

        Create.Table("Source")
            .WithColumn("SourceId").AsString().PrimaryKey()
            .WithColumn("Name").AsString().NotNullable().Unique()
            .WithColumn("IsSelected").AsBoolean().NotNullable().WithDefaultValue(1)
            .WithColumn("Documentation").AsString().Nullable()
            .WithColumn("TargetType").AsString().Nullable()
            .WithColumn("TargetName").AsString().Nullable()
            .WithColumn("ExportedBy").AsString().Nullable()
            .WithColumn("ExportedOn").AsDate().Nullable()
            .WithColumn("Content").AsString().NotNullable();

        Create.Table("Override")
            .WithColumn("SourceId").AsString().NotNullable().ForeignKey("Source", "SourceId").OnDelete(Rule.Cascade)
            .WithColumn("VariableId").AsString().NotNullable().ForeignKey("Variable", "VariableId").OnDelete(Rule.Cascade)
            .WithColumn("Data").AsString().NotNullable();

        Create.UniqueConstraint("Unique_Override_Source_Variable")
            .OnTable("Override")
            .Columns("SourceId", "VariableId");

        Create.Table("Run")
            .WithColumn("RunId").AsString().PrimaryKey()
            .WithColumn("SourceId").AsString().NotNullable().ForeignKey("Source", "SourceId").OnDelete(Rule.Cascade)
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Result").AsString().NotNullable()
            .WithColumn("RanOn").AsString().NotNullable()
            .WithColumn("RanBy").AsString().NotNullable();

        Create.Table("Outcome")
            .WithColumn("OutcomeId").AsString().PrimaryKey()
            .WithColumn("RunId").AsString().NotNullable().ForeignKey("Run", "RunId").OnDelete(Rule.Cascade)
            .WithColumn("NodeId").AsString().NotNullable().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Result").AsString().NotNullable()
            .WithColumn("Duration").AsInt32().NotNullable()
            .WithColumn("Evaluations").AsString();

        /*Create.Table("ChangeLog")
            .WithColumn("ChangeId").AsString().PrimaryKey()
            .WithColumn("NodeId").AsString().NotNullable().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("ChangeType").AsString().NotNullable()
            .WithColumn("ChangedOn").AsString().NotNullable()
            .WithColumn("ChangedBy").AsString().NotNullable();*/
    }
}