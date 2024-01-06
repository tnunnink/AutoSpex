using FluentMigrator.Runner;
using TestContext = AutoSpex.Persistence.Tests.TestContext;

namespace AutoSpex.Persistence.Tests.Migrations;

[TestFixture]
public class Migrator
{
    [Test]
    public void MigrationShouldProduceExpectedTablesFromTheMasterTable()
    {
        using var context = new TestContext();
        
        var migrator = context.Resolve<IMigrationRunner>();
        
        migrator.MigrateUp();
    }
}