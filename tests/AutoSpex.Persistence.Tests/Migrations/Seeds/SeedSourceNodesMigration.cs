using AutoSpex.Engine;
using FluentMigrator;

namespace AutoSpex.Persistence.Tests.Migrations.Seeds;

[MigrationId(100, 0, 0, "This is to seed some source nodes data for testing purposes")]
[Tags("SeedSourceNodesMigration")]
public class SeedSourceNodesMigration : Migration
{
    public override void Up()
    {
        Insert.IntoTable("Node").Row(GenerateNode("Source 1", NodeType.Source));
        Insert.IntoTable("Node").Row(GenerateNode("Source 2", NodeType.Source));
        Insert.IntoTable("Node").Row(GenerateNode("Source 3", NodeType.Source));
    }

    public override void Down()
    {
        Delete.FromTable("Node").AllRows();
    }

    private static dynamic GenerateNode(string name, NodeType nodeType)
    {
        return new
        {
            NodeId = Guid.NewGuid(),
            NodeType = nodeType,
            Name = name,
            Depth = 0,
            Ordinal = 0,
            Description = "Test Node"
        };
    }
}