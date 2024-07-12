using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace AutoSpex.Persistence.Tests;

[TestFixture]
public class RunMigration
{
    [Test]
    public void RunMigrationTest()
    {
        var provider = BuildServiceProvider();
        var runner = provider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
    
    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString("DataSource=app.db")
                .ScanIn(typeof(ServiceExtensions).Assembly).For.Migrations());

        return services.BuildServiceProvider();
    }
}