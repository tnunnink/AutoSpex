using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace AutoSpex.Persistence.Tests.Migrations;

public abstract class MigrationTest
{
    protected MigrationTest()
    {
        var provider = BuildServiceProvider();
        Runner = provider.GetRequiredService<IMigrationRunner>();
    }

    public IMigrationRunner Runner { get; }

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