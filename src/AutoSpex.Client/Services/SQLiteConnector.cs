using System;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;

namespace AutoSpex.Client.Services;

public abstract class SQLiteConnector : IDataConnector
{
    private readonly Func<string> _pathFactory;

    protected SQLiteConnector(Func<string> pathFactory)
    {
        _pathFactory = pathFactory;
    }

    public async Task<IDbConnection> Connect(CancellationToken token)
    {
        var connectionString = BuildConnectionString(_pathFactory());
        var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync(token);
        return connection;
    }
    
    private static string BuildConnectionString(string path)
    {
        var builder = new SQLiteConnectionStringBuilder
        {
            DataSource = path,
            ForeignKeys = true
        };

        return builder.ConnectionString;
    }
}