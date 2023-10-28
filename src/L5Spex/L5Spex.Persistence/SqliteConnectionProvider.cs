using System.Data;
using System.Data.SQLite;

namespace L5Spex.Persistence;

public class SqliteConnectionProvider : IConnectionProvider
{
    private readonly string _connectionString;

    public SqliteConnectionProvider(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public IDbConnection Connect() => new SQLiteConnection(_connectionString);
}