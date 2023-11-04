using System.Data;
using FluentMigrator;
using L5Spex.Client.Migrations;

namespace L5Spex.Migrations;

[MigrationId(1, 2023, 9, 8, 16, 56)]
public class M001_202309081656_InitialBuild : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("Source")
            .WithColumn("SourceId").AsGuid().PrimaryKey()
            .WithColumn("Path").AsString().NotNullable().Unique()
            .WithColumn("Selected").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("Pinned").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("Added").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentUTCDateTime);
        
        Create.Table("Node")
            .WithColumn("NodeId").AsGuid().PrimaryKey()
            .WithColumn("ParentId").AsGuid().Nullable().ForeignKey("Node", "NodeId")
            .WithColumn("NodeType").AsString().NotNullable()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Ordinal").AsInt32().NotNullable()
            .WithColumn("Created").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentUTCDateTime)
            .WithColumn("Modified").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentUTCDateTime);
    }
}