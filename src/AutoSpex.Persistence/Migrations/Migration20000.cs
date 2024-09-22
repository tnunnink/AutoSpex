using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[MigrationId(2, 00, 00, "Remove Variable Name Constraint")]
public class Migration20000 : Migration
{
    public override void Up()
    {
        Delete.UniqueConstraint("Unique_Variable_NodeId_Name").FromTable("Variable");
    }

    public override void Down()
    {
        //Do nothing here, as it would require data loss, and at the time of writing this application is still in
        //a development lifecycle. There shouldn't be any user that would need to downgrade to version 1 only.
    }
}