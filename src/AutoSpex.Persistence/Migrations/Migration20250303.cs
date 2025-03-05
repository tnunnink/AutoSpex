using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20250303, "Remove change table")]
public class Migration20250303 : Migration
{
    public override void Up()
    {
        Delete.Table("Change");
    }

    public override void Down()
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
}