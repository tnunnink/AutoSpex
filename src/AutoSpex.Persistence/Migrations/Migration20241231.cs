using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20241231, "Add Change table to track generic changes to entities in the system.")]
public class Migration20241231 : Migration
{
    public override void Up()
    {
        Create.Table("Change")
            .WithColumn("ChangeId").AsString().PrimaryKey()
            .WithColumn("EntityId").AsString().NotNullable()
            .WithColumn("Request").AsString().NotNullable()
            .WithColumn("ChangeType").AsString().NotNullable()
            .WithColumn("ChangedOn").AsString().NotNullable()
            .WithColumn("ChangedBy").AsString().NotNullable()
            .WithColumn("Message").AsString();
    }

    public override void Down()
    {
        Delete.Table("Change");
    }
}