using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;

namespace AutoSpex.Persistence.Tests.Migrations;

public abstract class MigrationTest
{
    protected MigrationTest()
    {
        var provider = BuildServiceProvider();
        Runner = provider.GetRequiredService<IMigrationRunner>();
    }

    protected IMigrationRunner Runner { get; }

    protected static void CleanUp()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        if (File.Exists("app.db")) File.Delete("app.db");
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString("DataSource=app.db")
                .ScanIn(typeof(ServiceExtensions).Assembly).For.Migrations()
                .ScanIn(typeof(MigrationTest).Assembly).For.Migrations()
            ).Configure<RunnerOptions>(cfg => { cfg.Profile = "Development"; });

        return services.BuildServiceProvider();
    }
}