using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace AutoSpex.Client.Services;

public interface IDataConnector
{
    Task<IDbConnection> Connect(CancellationToken token);
}