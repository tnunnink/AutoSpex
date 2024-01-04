using FluentMigrator.Runner;

namespace AutoSpex.Client.Tests.Migrations;

[TestFixture]
public class Migrator
{
    [Test]
    public void MigrationShouldProduceExpectedTablesFromTheMasterTable()
    {
        using var context = new TestContext();
        
        var migrator = Resolve<IMigrationRunner>();
        
        migrator.MigrateUp();
    }
}