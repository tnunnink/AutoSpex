using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace L5Spex.Persistence;

public class SqliteMigrationProvider : IMigrationProvider
{
    private readonly string _connectionString;

    public SqliteMigrationProvider(string connectionString)
    {
        _connectionString = connectionString;
    }
    public void Migrate()
    {
        var service = new ServiceCollection();
        
        var provider = service.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(_connectionString)
                .ScanIn(typeof(SqliteMigrationProvider).Assembly).For.Migrations())
            .BuildServiceProvider();

        var migrator = provider.GetRequiredService<IMigrationRunner>();
        migrator.MigrateUp();
    }
}