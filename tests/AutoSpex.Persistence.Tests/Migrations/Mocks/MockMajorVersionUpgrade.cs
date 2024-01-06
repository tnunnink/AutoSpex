using FluentMigrator;

namespace AutoSpex.Persistence.Tests.Migrations.Mocks;

[MigrationId(2, 0, 0, "This is to test a new major version of the database")]
[Tags("MockMajorVersionUpgrade")]
public class MockMajorVersionUpgrade : Migration
{
    public override void Up()
    {
        Create.Table("Test").WithColumn("Id").AsGuid().PrimaryKey();
    }

    public override void Down()
    {
        Delete.Table("Test");
    }
}