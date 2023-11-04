using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace L5Spex.Client.Tests;

[TestFixture]
public class MigrationRunner
{
    private const string TestDatabase = @"C:\Projects\L5Spex\src\L5Spex\L5Spex.Client\bin\debug\net6.0\spex.db";

    [Test]
    public void RunAllMigrationsToCreateTestDatabaseForQueryStringResolution()
    {
        if (File.Exists(TestDatabase)) File.Delete(TestDatabase);
        
        var service = new ServiceCollection();
        
        var provider = service.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString($"Data Source={TestDatabase};")
                .ScanIn(typeof(Bootstrapper).Assembly).For.Migrations())
            .BuildServiceProvider();

        var migrator = provider.GetRequiredService<IMigrationRunner>();
        migrator.MigrateUp();
        
        FileAssert.Exists(TestDatabase);
    }
}