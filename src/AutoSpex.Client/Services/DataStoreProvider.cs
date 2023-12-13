using System;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using JetBrains.Annotations;

namespace AutoSpex.Client.Services;

[UsedImplicitly]
public class DataStoreProvider : IDataStoreProvider
{
    private readonly ISettingsManager _settings;

    public DataStoreProvider(ISettingsManager settings)
    {
        _settings = settings;
    }

    public async Task<IDbConnection> ConnectTo(StoreType storeType, CancellationToken token)
    {
        var connectionString = storeType switch
        {
            StoreType.Application => _settings.Get(Setting.AppConnection),
            StoreType.Project => _settings.Get(Setting.OpenProjectConnection),
            _ => throw new ArgumentOutOfRangeException(nameof(storeType), "Can not process provided store type.")
        };
        
        var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync(token);
        return connection;
    }
}