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
            .WithColumn("Feature").AsString().NotNullable()
            .WithColumn("NodeType").AsString().NotNullable()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Depth").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("Ordinal").AsInt32().NotNullable().WithDefaultValue(0);

        Create.Table("Source")
            .WithColumn("NodeId").AsString().PrimaryKey().ForeignKey("Node", "NodeId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("Controller").AsString().Nullable()
            .WithColumn("Processor").AsString().Nullable()
            .WithColumn("Revision").AsString().Nullable()
            .WithColumn("IsContext").AsString().Nullable()
            .WithColumn("TargetType").AsString().Nullable()
            .WithColumn("TargetName").AsString().Nullable()
            .WithColumn("ExportedBy").AsString().Nullable()
            .WithColumn("ExportedOn").AsDate().Nullable()
            .WithColumn("Content").AsString().NotNullable();
        
        Create.Table("Spec")
            .WithColumn("NodeId").AsString().PrimaryKey().ForeignKey("Node", "NodeId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("Element").AsString().NotNullable();
        
        Create.Table("ChangeLog")
            .WithColumn("NodeId").AsString().PrimaryKey().ForeignKey("Node", "NodeId").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("ChangeType").AsInt16().NotNullable()
            .WithColumn("ChangedOn").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("Message").AsString().Nullable();
    }
}