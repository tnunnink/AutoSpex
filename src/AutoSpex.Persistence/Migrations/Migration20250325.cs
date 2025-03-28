using System.Data;
using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20250325, "Remove Source. Add Repo. Replace Reference")]
public class Migration20250325 : Migration
{
    public override void Up()
    {
        Delete.Table("Source");
        Delete.Table("Reference");

        Create.Table("Repo")
            .WithColumn("RepoId").AsString().PrimaryKey()
            .WithColumn("Location").AsString().NotNullable()
            .WithColumn("Name").AsString().NotNullable();

        Create.Table("Reference")
            .WithColumn("Scope").AsString().PrimaryKey()
            .WithColumn("RepoId").AsString().ForeignKey("Node", "NodeId").OnDelete(Rule.Cascade)
            .WithColumn("Element").AsString().NotNullable()
            .WithColumn("Content").AsString().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Repo");
        Delete.Table("Reference");

        Create.Table("Source")
            .WithColumn("SourceId").AsString().PrimaryKey()
            .WithColumn("Name").AsString().NotNullable().Unique()
            .WithColumn("IsTarget").AsBoolean().NotNullable().WithDefaultValue(0)
            .WithColumn("TargetType").AsString().Nullable()
            .WithColumn("TargetName").AsString().Nullable()
            .WithColumn("ExportedOn").AsDateTime().Nullable()
            .WithColumn("ExportedBy").AsString().Nullable()
            .WithColumn("Description").AsString().Nullable()
            .WithColumn("Content").AsString().Nullable();

        Create.Table("Reference")
            .WithColumn("Scope").AsString().NotNullable().Unique();
    }
}