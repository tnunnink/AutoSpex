using System.Data;
using System.Data.SQLite;

namespace L5Spex.Client.Services;

public class SqliteDatabaseProvider : IDatabaseProvider
{
    private readonly string _connectionString;

    public SqliteDatabaseProvider(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public IDbConnection Connect() => new SQLiteConnection(_connectionString);
}