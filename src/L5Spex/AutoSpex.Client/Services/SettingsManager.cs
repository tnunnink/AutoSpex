using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using Dapper;
using JetBrains.Annotations;

namespace AutoSpex.Client.Services;

[UsedImplicitly]
public sealed class SettingsManager : ISettingsManager
{
    private const string Upsert =
        "INSERT INTO Setting (Key, Value) VALUES (@Key, @Value) ON CONFLICT DO UPDATE SET Value = @Value";

    private readonly IDbConnection _connection;

    private SettingsManager(IDbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public static SettingsManager Load(string connectionString)
    {
        var connection = new SQLiteConnection(connectionString);
        connection.Open();
        return new SettingsManager(connection);
    }

    public string Get(Setting setting)
    {
        return _connection.QuerySingle<string>("SELECT Value FROM Setting WHERE Key = @Key",
            new {Key = setting.ToString()});
    }

    public T Get<T>(Setting setting, Func<string, T> convert)
    {
        var value = _connection.QuerySingle<string>("SELECT Value FROM Setting WHERE Key = @Key",
            new {Key = setting.ToString()});

        return convert(value);
    }

    public async void Save(Setting setting, string value)
    {
        await _connection.ExecuteAsync(Upsert, new {Key = setting.ToString(), Value = value});
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is IAsyncDisposable connectionAsyncDisposable)
            await connectionAsyncDisposable.DisposeAsync();
        else
            _connection.Dispose();
    }
}