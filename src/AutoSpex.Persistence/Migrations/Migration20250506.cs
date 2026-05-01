using System.Data;
using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20250506, "Add target table")]
public class Migration20250506 : Migration
{
    public override void Up()
    {
        Create.Table("Target")
            .WithColumn("TargetId").AsString().PrimaryKey()
            .WithColumn("RepoId").AsString().ForeignKey("Repo", "RepoId").OnDelete(Rule.Cascade)
            .WithColumn("Location").AsString().NotNullable();

        Create.UniqueConstraint("UNQ_Target_Repo_Location").OnTable("Target").Columns("RepoId", "Location");
    }

    public override void Down()
    {
        Delete.Table("Target");
    }
}