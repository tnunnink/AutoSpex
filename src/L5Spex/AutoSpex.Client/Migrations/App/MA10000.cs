using FluentMigrator;

namespace AutoSpex.Client.Migrations.App;

[MigrationId(1, 00, 00, "Initial Build")]
[Tags("App")]
public class MA10000 : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("Project")
            .WithColumn("Path").AsString().PrimaryKey()
            .WithColumn("OpenedOn").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("Pinned").AsBoolean().NotNullable().WithDefaultValue(false);

        Create.Table("Setting")
            .WithColumn("Key").AsString().PrimaryKey()
            .WithColumn("Value").AsString().NotNullable();
    }
}