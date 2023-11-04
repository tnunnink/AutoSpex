using FluentMigrator.Runner;
using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace L5Spex.Client.Tests;

public static class Testing
{
    public static IContainer SetupEnvironment()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        var configuration = builder.Build();
        
        //Remove current test database each time we setup from previous runs if it exists.
        var fileName = configuration["DatabaseFile"];
        if (File.Exists(fileName)) File.Delete(fileName);
        
        var container = Bootstrapper.Build(builder.Build());

        RunMigrations(configuration.GetConnectionString("DefaultConnection")!);

        return container;
    }

    private static void RunMigrations(string connectionString)
    {
        var registry = new ServiceRegistry();
        
        var provider = registry.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(Testing).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .BuildServiceProvider();
        
        using var scope = provider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
    
}