using System.Data;
using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20241127, "Added Reference table")]
public class Migration20241127 : Migration
{
    public override void Up()
    {
        Create.Table("Reference")
            .WithColumn("ReferenceId").AsString().PrimaryKey()
            .WithColumn("SourceId").AsString().NotNullable().ForeignKey("Source", "SourceId").OnDelete(Rule.Cascade)
            .WithColumn("Element").AsString().NotNullable()
            .WithColumn("Scope").AsString().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Reference");
    }
}