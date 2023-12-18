using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Migrations;
using FluentMigrator;

namespace AutoSpex.Client.Tests.Migrations.Seeds;

[MigrationId(100, 0, 0, "This is to seed some collection nodes data for testing purposes")]
[Tags("SeedCollectionNodesMigration")]
public class SeedCollectionNodesMigration : Migration
{
    public override void Up()
    {
        Insert.IntoTable("Node").Row(new
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Feature = Feature.Specifications.Name,
            NodeType = NodeType.Collection.Name,
            Name = "Collection 1"
        });
        
        Insert.IntoTable("Node").Row(new
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Feature = Feature.Specifications.Name,
            NodeType = NodeType.Collection.Name,
            Name = "Collection 2"
        });
        
        Insert.IntoTable("Node").Row(new
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Feature = Feature.Specifications.Name,
            NodeType = NodeType.Collection.Name,
            Name = "Collection 3"
        });
    }

    public override void Down()
    {
        Delete.FromTable("Node").AllRows();
    }
}