using System.Data;
using System.Data.SQLite;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;

namespace AutoSpex.Persistence;

public class ConnectionManager : IConnectionManager
{
    private const string DefaultAppDatabase = "app.db";
    private readonly Dictionary<Database, string> _connections = new();

    private ConnectionManager()
    {
    }

    public static IConnectionManager Default => DefaultInstance();

    public void Register(Database database, string dataSource)
    {
        if (!_connections.TryAdd(database, dataSource))
            _connections[database] = dataSource;
    }

    public bool IsRegistered(Database database) => _connections.ContainsKey(database);

    public string GetSource(Database database)
    {
        if (!_connections.TryGetValue(database, out var dataSource))
            throw new InvalidOperationException($"No source path is defined for the database '{database}'");

        return dataSource;
    }

    public async Task<IDbConnection> Connect(Database database, CancellationToken token)
    {
        var created = EnsureCreated(database);
        if (created.IsFailed)
            throw new InvalidOperationException(
                "Could not open database connection due to migration failure. " +
                "Ensure that the database has been migrated or all connection are closed before connecting.");
        
        var connectionString = BuildConnectionString(database);
        var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync(token);
        return connection;
    }

    public Result Migrate(Database database, long version = 0)
    {
        return Result.Try(() =>
        {
            using var provider = BuildServiceProvider(database);
            using var scope = provider.CreateScope();
            var runner = provider.GetRequiredService<IMigrationRunner>();

            if (version > 0)
            {
                runner.MigrateUp(version);
                return;
            }

            runner.MigrateUp();
            
        }, e => new Error($"Migration of '{database}' database failed with error {e.Message}.").CausedBy(e));
    }
    
    private static IConnectionManager DefaultInstance()
    {
        var manager = new ConnectionManager();
        manager.Register(Database.App, DefaultAppDatabase);
        return manager;
    }
    
    private Result EnsureCreated(Database database)
    {
        var dataSource = GetDataSource(database);
        if (File.Exists(dataSource)) return Result.Ok();
        return Migrate(database);
    }

    private ServiceProvider BuildServiceProvider(Database database)
    {
        var connectionString = BuildConnectionString(database);
        
        var services = new ServiceCollection();

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(ConnectionManager).Assembly).For.Migrations())
            /*.AddLogging(lb => lb.AddSerilog())*/
            .Configure<RunnerOptions>(opt => { opt.Tags = [database.ToString()]; });

        return services.BuildServiceProvider();
    }

    private string BuildConnectionString(Database database)
    {
        var dataSource = GetDataSource(database);

        var builder = new SQLiteConnectionStringBuilder
        {
            DataSource = dataSource,
            ForeignKeys = true,
            Pooling = false
        };

        return builder.ConnectionString;
    }

    private string GetDataSource(Database database)
    {
        if (!_connections.TryGetValue(database, out var dataSource))
            throw new InvalidOperationException(
                $"No data source path is registered for database '{database}'. " +
                $"Must call Register prior to connecting or migrating the requested database.");

        return dataSource;
    }
}