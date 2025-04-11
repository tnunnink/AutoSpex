using System.Data;
using System.Data.SQLite;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace AutoSpex.Persistence;

public class ConnectionManager(string dataSource) : IConnectionManager
{
    /*private static readonly string AppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public static string SpexDb = Path.Combine(AppData, "AutoSpex", "spex.db");*/
    private const string AppDatabase = "../spex.db";

    /// <summary>
    /// Gets the default implementation of the <see cref="IConnectionManager"/> interface.
    /// </summary>
    /// <remarks>
    /// This property provides a pre-configured instance of <see cref="ConnectionManager"/> targeting the default application database.
    /// </remarks>
    public static IConnectionManager Default => new ConnectionManager(AppDatabase);

    /// <inheritdoc />
    public async Task<IDbConnection> Connect(CancellationToken token)
    {
        var connectionString = BuildConnectionString(dataSource);
        Migrate(connectionString);
        var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync(token);
        return connection;
    }

    /// <summary>
    /// Applies database migrations to ensure the database schema is up-to-date.
    /// </summary>
    private static void Migrate(string connectionString)
    {
        using var provider = BuildServiceProvider(connectionString);
        using var scope = provider.CreateScope();
        var runner = provider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    /// <summary>
    /// Builds and configures a service provider with FluentMigrator services using the specified
    /// connection string and database.
    /// </summary>
    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        var services = new ServiceCollection();

        services.AddFluentMigratorCore()
            .ConfigureRunner(builder => builder
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(ConnectionManager).Assembly).For.Migrations());
        //.Configure<RunnerOptions>(o => { o.Tags = [database]; });

        return services.BuildServiceProvider(false);
    }

    /// <summary>
    /// Builds the required SQLite connection string using the path the provided data source.
    /// </summary>
    private static string BuildConnectionString(string dataSource)
    {
        var builder = new SQLiteConnectionStringBuilder
        {
            DataSource = dataSource,
            ForeignKeys = true,
            Pooling = false
        };

        return builder.ConnectionString;
    }
}