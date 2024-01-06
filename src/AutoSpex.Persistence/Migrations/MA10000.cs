using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[MigrationId(1, 00, 00, "Initial Build")]
[Tags("App")]
public class MA10000 : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("Project")
            .WithColumn("Path").AsString().PrimaryKey()
            .WithColumn("OpenedOn").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);
    }
}