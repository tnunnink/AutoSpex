using System.Data;
using System.Data.SQLite;

namespace L5Spex.Services;

public interface IConnectionProvider
{
    IDbConnection Connect();
}

public class ConnectionProvider : IConnectionProvider
{
    private readonly string _connectionString;

    public ConnectionProvider(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public IDbConnection Connect() => new SQLiteConnection(_connectionString);
}