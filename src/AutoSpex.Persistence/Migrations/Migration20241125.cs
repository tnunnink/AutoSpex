using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20241125, "Added unique name constraint to Source")]
public class Migration20241125 : Migration
{
    public override void Up()
    {
        Create.UniqueConstraint("UQ_Source_Name").OnTable("Source").Column("Name");
    }

    public override void Down()
    {
        Delete.UniqueConstraint("UQ_Source_Name").FromTable("Source").Column("Name");
    }
}