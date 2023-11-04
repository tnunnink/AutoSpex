using System.Data;

namespace L5Spex.Client.Services;

public interface IDatabaseProvider
{
    IDbConnection Connect();
}