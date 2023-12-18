using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
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

    private readonly Dictionary<Setting, string> _settings;
    private readonly IDbConnection _connection;

    private SettingsManager(IDbConnection connection, Dictionary<Setting, string> settings)
    {
        _connection = connection;
        _settings = settings;
    }

    public static ISettingsManager Load(string connectionString)
    {
        var connection = new SQLiteConnection(connectionString);
        connection.Open();
        
        var results = connection.Query("SELECT * FROM Setting").ToList();

        var settings = new Dictionary<Setting, string>();
        
        foreach (var result in results)
        {
            var key = Enum.Parse<Setting>(result.Key);
            settings.TryAdd(key, result.Value);
        }
        
        return new SettingsManager(connection, settings);
    }

    public void Add<T>(Setting setting, T value)
    {
        var text = value?.ToString();
        
        if (text is null)
            throw new ArgumentException("Can not add setting with null value.");

        _settings.TryAdd(setting, text);
    }

    public string Get(Setting setting)
    {
        return _settings[setting];
    }

    public T Get<T>(Setting setting, Func<string, T> convert)
    {
        return convert(_settings[setting]);
    }

    public string? Find(Setting setting)
    {
        return _settings.GetValueOrDefault(setting);
    }

    public T? Find<T>(Setting setting, Func<string, T> convert)
    {
        return _settings.TryGetValue(setting, out var value) ? convert(value) : default;
    }

    public void Set<T>(Setting setting, T? value)
    {
        var text = value?.ToString();
        if (text is null)
        {
            _settings.Remove(setting);
            return;
        }
        
        if (!_settings.TryAdd(setting, text))
            _settings[setting] = text;
    }

    public async Task Save()
    {
        using var transaction = _connection.BeginTransaction();
        
        foreach (var setting in _settings)
        {
            await _connection.ExecuteAsync(Upsert, new { Key = setting.Key.ToString(), setting.Value }, transaction);
        }
        
        transaction.Commit();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        if (_connection is IAsyncDisposable connectionAsyncDisposable)
            return connectionAsyncDisposable.DisposeAsync();

        _connection.Dispose();
        return ValueTask.CompletedTask;
    }
}