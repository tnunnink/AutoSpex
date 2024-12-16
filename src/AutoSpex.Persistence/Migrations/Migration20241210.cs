using FluentMigrator;
using JetBrains.Annotations;

namespace AutoSpex.Persistence;

[UsedImplicitly]
[Migration(20241210, "Renamed Comment to Description for Node table.")]
public class Migration20241210 : Migration
{
    public override void Up()
    {
        Rename.Column("Comment").OnTable("Node").InSchema("main").To("Description");
    }

    public override void Down()
    {
        Rename.Column("Description").OnTable("Node").InSchema("main").To("Comment");
    }
}