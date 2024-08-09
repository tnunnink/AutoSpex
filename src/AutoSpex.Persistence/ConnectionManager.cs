using System.Data;
using System.Data.SQLite;

namespace AutoSpex.Persistence;

public class ConnectionManager : IConnectionManager
{
    private const string AppDatabase = "spex.db";
    public static readonly string ConnectionString = BuildConnectionString();

    /// <inheritdoc />
    public async Task<IDbConnection> Connect(CancellationToken token)
    {
        var connectionString = BuildConnectionString();
        var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync(token);
        return connection;
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