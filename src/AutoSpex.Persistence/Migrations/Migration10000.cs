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

        Create.Table("Source")
            .WithColumn("SourceId").AsString().PrimaryKey()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("IsTarget").AsBoolean().NotNullable().WithDefaultValue(0)
            .WithColumn("TargetType").AsString().Nullable()
            .WithColumn("TargetName").AsString().Nullable()
            .WithColumn("ExportedOn").AsDateTime().Nullable()
            .WithColumn("ExportedBy").AsString().Nullable()
            .WithColumn("Description").AsString().Nullable()
            .WithColumn("Content").AsString().Nullable();

        Create.Table("Override")
            .WithColumn("OverrideId").AsString().PrimaryKey()
            .WithColumn("SourceId").AsString().ForeignKey("Source", "SourceId").OnDelete(Rule.Cascade)
            .WithColumn("VariableId").AsString().ForeignKey("Variable", "VariableId").OnDelete(Rule.Cascade)
            .WithColumn("Value").AsString().NotNullable();

        Create.Table("Run")
            .WithColumn("RunId").AsString().PrimaryKey()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Node").AsString().NotNullable()
            .WithColumn("Source").AsString().NotNullable()
            .WithColumn("Result").AsString().NotNullable().WithDefaultValue(ResultState.None)
            .WithColumn("RanOn").AsString().NotNullable()
            .WithColumn("RanBy").AsString().NotNullable()
            .WithColumn("Outcomes").AsString().Nullable();
    }
}