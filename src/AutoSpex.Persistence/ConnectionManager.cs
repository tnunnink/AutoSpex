using System.Data;
using System.Data.SQLite;
using Dapper;
using FluentResults;

namespace AutoSpex.Persistence;

public class ConnectionManager : IConnectionManager
{
    
    private const string AppDatabase = "../spex.db";
    public static readonly string ConnectionString = BuildConnectionString();

    /// <inheritdoc />
    public async Task<IDbConnection> Connect(CancellationToken token)
    {
        var connectionString = BuildConnectionString();
        var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync(token);
        return connection;
    }

    /// <inheritdoc />
    public Task Vacuum(IDbConnection connection)
    {
        return connection.ExecuteAsync("VACUUM");
    }

    /// <summary>
    /// Builds the required connection string to our application database.
    /// </summary>
    private static string BuildConnectionString()
    {
        var builder = new SQLiteConnectionStringBuilder
        {
            DataSource = AppDatabase,
            ForeignKeys = true,
            Pooling = false
        };

        return builder.ConnectionString;
    }
}