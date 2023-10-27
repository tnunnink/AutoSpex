using System.Data;

namespace L5Spex.Persistence;

public interface IConnectionProvider
{
    IDbConnection Connect();
}