using System.Data;
using AutoSpex.Engine;
using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[MigrationId(1, 00, 00, "Initial Build")]
public class Migration10000 : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("Node")
            .WithColumn("NodeId").AsString().PrimaryKey()
            .WithColumn("ParentId").AsString().Nullable().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Type").AsString().NotNullable()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Comment").AsString().Nullable();

        Create.Table("Spec")
            .WithColumn("SpecId").AsString().PrimaryKey()
            .WithColumn("NodeId").AsString().NotNullable().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Config").AsString().NotNullable();

        Create.Table("Variable")
            .WithColumn("VariableId").AsString().PrimaryKey()
            .WithColumn("NodeId").AsString().NotNullable().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Group").AsString().NotNullable()
            .WithColumn("Value").AsString().Nullable();

        Create.UniqueConstraint("Unique_Variable_NodeId_Name")
            .OnTable("Variable")
            .Columns("NodeId", "Name");

        Create.Table("Environment")
            .WithColumn("EnvironmentId").AsString().PrimaryKey()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Comment").AsString().Nullable()
            .WithColumn("IsTarget").AsBoolean().NotNullable().WithDefaultValue(0);

        Create.Table("Source")
            .WithColumn("SourceId").AsString().PrimaryKey()
            .WithColumn("EnvironmentId").AsString().ForeignKey("Environment", "EnvironmentId").OnDelete(Rule.Cascade)
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Uri").AsString().NotNullable();

        Create.Table("Override")
            .WithColumn("OverrideId").AsString().PrimaryKey()
            .WithColumn("SourceId").AsString().ForeignKey("Source", "SourceId").OnDelete(Rule.Cascade)
            .WithColumn("VariableId").AsString().ForeignKey("Variable", "VariableId").OnDelete(Rule.Cascade)
            .WithColumn("Value").AsString().NotNullable();

        Create.Table("Run")
            .WithColumn("RunId").AsString().PrimaryKey()
            .WithColumn("EnvironmentId").AsString().ForeignKey("Environment", "EnvironmentId").OnDelete(Rule.Cascade)
            .WithColumn("Result").AsString().Nullable().WithDefaultValue(ResultState.None)
            .WithColumn("RanOn").AsString().Nullable()
            .WithColumn("RanBy").AsString().Nullable();

        Create.Table("Outcome")
            .WithColumn("OutcomeId").AsString().PrimaryKey()
            .WithColumn("RunId").AsString().ForeignKey("Run", "RunId").OnDelete(Rule.Cascade)
            .WithColumn("NodeId").AsString().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Result").AsString().Nullable().WithDefaultValue(ResultState.None)
            .WithColumn("Duration").AsInt32().Nullable().WithDefaultValue(0)
            .WithColumn("Evaluations").AsString().Nullable();
    }
}