using System.Data;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Services;

public interface IDataStoreProvider
{
    Task<IDbConnection> ConnectTo(StoreType storeType, CancellationToken token);
}