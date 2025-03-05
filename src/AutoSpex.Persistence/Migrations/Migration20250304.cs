using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20250304, "Added Log table.")]
public class Migration20250304: Migration
{
    public override void Up()
    {
        Create.Table("Log")
            .WithColumn("LogId").AsInt64().PrimaryKey()
            .WithColumn("Logged").AsString().NotNullable()
            .WithColumn("Logger").AsString().NotNullable()
            .WithColumn("Level").AsString().NotNullable()
            .WithColumn("Message").AsString().NotNullable()
            .WithColumn("Callsite").AsString().NotNullable()
            .WithColumn("Machine").AsString().NotNullable()
            .WithColumn("Properties").AsString().Nullable()
            .WithColumn("Exception").AsString().Nullable();
    }

    public override void Down()
    {
        Delete.Table("Log");
    }
}