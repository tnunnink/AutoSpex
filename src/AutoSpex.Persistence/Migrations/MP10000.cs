using System.Data;
using AutoSpex.Engine;
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
            .WithColumn("ParentId").AsString().Nullable().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Type").AsString().NotNullable()
            .WithColumn("Name").AsString().NotNullable();

        Create.Table("Spec")
            .WithColumn("SpecId").AsString().PrimaryKey().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Element").AsString().NotNullable().WithDefaultValue(Element.Default)
            .WithColumn("Specification").AsString().NotNullable().WithDefaultValue(new Spec().Serialize());

        Create.Table("Source")
            .WithColumn("SourceId").AsString().PrimaryKey().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("TargetType").AsString().Nullable()
            .WithColumn("TargetName").AsString().Nullable()
            .WithColumn("ExportedBy").AsString().Nullable()
            .WithColumn("ExportedOn").AsDate().Nullable()
            .WithColumn("Content").AsString().Nullable();

        Create.Table("Run")
            .WithColumn("RunId").AsString().PrimaryKey().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("SourceId").AsString().Nullable().ForeignKey("Source", "SourceId").OnDelete(Rule.SetNull)
            .WithColumn("Result").AsString().NotNullable().WithDefaultValue(ResultState.None)
            .WithColumn("RanOn").AsString().Nullable()
            .WithColumn("RanBy").AsString().Nullable();

        Create.Table("Outcome")
            .WithColumn("OutcomeId").AsString().PrimaryKey()
            .WithColumn("RunId").AsString().NotNullable().ForeignKey("Run", "RunId").OnDelete(Rule.Cascade)
            .WithColumn("SpecId").AsString().NotNullable().ForeignKey("Spec", "SpecId").OnDelete(Rule.Cascade)
            .WithColumn("Result").AsString().NotNullable().WithDefaultValue(ResultState.None)
            .WithColumn("Duration").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("Evaluations").AsString().Nullable();

        Create.Table("Variable")
            .WithColumn("VariableId").AsString().PrimaryKey()
            .WithColumn("NodeId").AsString().NotNullable().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Group").AsString().NotNullable()
            .WithColumn("Type").AsString().Nullable()
            .WithColumn("Data").AsString().Nullable()
            .WithColumn("Description").AsString().Nullable();

        Create.UniqueConstraint("Unique_Variable_NodeId_Name")
            .OnTable("Variable")
            .Columns("NodeId", "Name");

        Create.Table("ChangeLog")
            .WithColumn("ChangeId").AsString().PrimaryKey()
            .WithColumn("NodeId").AsString().NotNullable().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("CommandName").AsString().NotNullable()
            .WithColumn("ChangedOn").AsString().NotNullable()
            .WithColumn("ChangedBy").AsString().NotNullable()
            .WithColumn("Comment").AsString().NotNullable();

        Create.Table("Documentation")
            .WithColumn("NodeId").AsString().PrimaryKey().ForeignKey("Node", "NodeId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("Content").AsString().NotNullable();

        //Seed default root nodes to which all nodes will be added. These are the only nodes with null parent.
        //This is how we identify which child belongs to which "feature"
        Insert.IntoTable("Node")
            .Row(new { Nodeid = Guid.NewGuid().ToString(), Type = NodeType.Spec.Name, Name = "Root" })
            .Row(new { Nodeid = Guid.NewGuid().ToString(), Type = NodeType.Source.Name, Name = "Root" })
            .Row(new { Nodeid = Guid.NewGuid().ToString(), Type = NodeType.Run.Name, Name = "Root" });
    }
}