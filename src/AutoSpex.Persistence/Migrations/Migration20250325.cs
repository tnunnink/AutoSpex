using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20250325, "Remove Source. Add Repo")]
public class Migration20250325 : Migration
{
    public override void Up()
    {
        Delete.Table("Source");

        Create.Table("Repo")
            .WithColumn("RepoId").AsString().PrimaryKey()
            .WithColumn("Location").AsString().NotNullable().Unique()
            .WithColumn("LastConnected").AsString().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);
    }

    public override void Down()
    {
        Delete.Table("Repo");

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
    }
}