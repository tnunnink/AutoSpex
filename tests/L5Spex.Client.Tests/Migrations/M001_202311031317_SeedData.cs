using System.Dynamic;
using Bogus;
using FluentMigrator;
using L5Spex.Client.Common;
using L5Spex.Client.Migrations;

namespace L5Spex.Client.Tests.Migrations;

[MigrationId(1, 2023, 11, 3, 13, 17)]
public class M001_202311031317_SeedData : Migration
{
    public override void Up()
    {
        var project = new {NodeId = Guid.NewGuid(), ParentId = (Guid?) null, NodeType = NodeType.Project, Name = "MyProject", Ordinal = 0};
        var folder = new {NodeId = Guid.NewGuid(), ParentId = project.NodeId, NodeType = NodeType.Folder, Name = "FolderName", Ordinal = 0};
        var spec1 = new {NodeId = Guid.NewGuid(), ParentId = folder.NodeId, NodeType = NodeType.Specification, Name = "MyFirstSpec", Ordinal = 0};
        var spec2 = new {NodeId = Guid.NewGuid(), ParentId = folder.NodeId, NodeType = NodeType.Specification, Name = "MySecondSpec", Ordinal = 1};
        var spec3 = new {NodeId = Guid.NewGuid(), ParentId = folder.NodeId, NodeType = NodeType.Specification, Name = "MyThridSpec", Ordinal = 2};

        Insert.IntoTable("Node").Row(project);
        Insert.IntoTable("Node").Row(folder);
        Insert.IntoTable("Node").Row(spec1);
        Insert.IntoTable("Node").Row(spec2);
        Insert.IntoTable("Node").Row(spec3);
    }

    public override void Down()
    {
        Delete.FromTable("Node").AllRows();
    }
}