using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Styling;
using Dapper;

namespace AutoSpex.Client;

public sealed class Settings
{
    #region Constants

    private const string ConnectionString = "Data Source=settings.db;Pooling=False;";

    private const string CreateTable =
        "CREATE TABLE Setting (Key TEXT NOT NULL, Value TEXT NOT NULL, CONSTRAINT PK_Setting PRIMARY KEY (Key))";

    private const string TableExists =
        "SELECT count() from sqlite_master WHERE tbl_name = 'Setting' AND type = 'table'";

    private const string Upsert =
        "INSERT INTO Setting (Key, Value) VALUES (@Key, @Value) ON CONFLICT DO UPDATE SET Value = @Value";

    #endregion

    private static readonly Dictionary<string, object> Defaults = new();

    private readonly Lazy<Dictionary<string, string>> _cache = new(Load,
        LazyThreadSafetyMode.ExecutionAndPublication);

    /*private readonly Dictionary<string, string> _changes = new();*/

    private static Settings? _settings;

    private Settings()
    {
        AddDefault(nameof(Theme), ThemeVariant.Light);
        AddDefault(nameof(ShellHeight), 800);
        AddDefault(nameof(ShellWidth), 1400);
        AddDefault(nameof(ShellState), WindowState.Normal);
        AddDefault(nameof(AlwaysDiscardChanges), false);
    }

    public static Settings App => _settings ??= new Settings();

    #region Settings

    public ThemeVariant Theme
    {
        get => GetSetting<ThemeVariant>(s =>
        {
            return s switch
            {
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                _ => ThemeVariant.Default
            };
        });
        set => SetSetting(value);
    }

    public double ShellHeight
    {
        get => GetSetting(double.Parse);
        set => SetSetting(value);
    }

    public double ShellWidth
    {
        get => GetSetting(double.Parse);
        set => SetSetting(value);
    }

    public int ShellX
    {
        get => GetSetting(int.Parse);
        set => SetSetting(value);
    }

    public int ShellY
    {
        get => GetSetting(int.Parse);
        set => SetSetting(value);
    }

    public WindowState ShellState
    {
        get => GetSetting(Enum.Parse<WindowState>);
        set => SetSetting(value);
    }
    
    public bool AlwaysDiscardChanges
    {
        get => GetSetting(s => s == "1");
        set => SetSetting(value);
    }

    #endregion

    public void Save(Action<Settings> update)
    {
        update(App);
        SaveSettings();
    }

    public async Task SaveAsync(Action<Settings> update)
    {
        update(App);
        await SaveSettingsAsync();
    }

    private static void AddDefault(string setting, object value)
    {
        Defaults.TryAdd(setting, value);
    }

    private T GetSetting<T>(Func<string, T> parser, [CallerMemberName] string? setting = default)
    {
        if (setting is null) throw new ArgumentNullException(nameof(setting));
        if (parser is null) throw new ArgumentNullException(nameof(parser));

        if (_cache.Value.TryGetValue(setting, out var value))
        {
            return parser.Invoke(value);
        }

        throw new InvalidOperationException($"No value for setting '{setting}' is defined");
    }

    private void SetSetting<T>(T value, [CallerMemberName] string? setting = default)
    {
        if (setting is null) throw new ArgumentNullException(nameof(setting));
        if (value is null) throw new ArgumentNullException(nameof(value));

        var s = value.ToString() ?? throw new ArgumentException("Can not convert value to string");

        if (!_cache.Value.TryAdd(setting, s))
            _cache.Value[setting] = s;
    }

    private void SaveSettings()
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        foreach (var setting in _cache.Value)
        {
            connection.Execute(Upsert, new {setting.Key, setting.Value}, transaction);
        }

        transaction.Commit();
    }

    private async Task SaveSettingsAsync()
    {
        await using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        await using var transaction = connection.BeginTransaction();

        foreach (var setting in _cache.Value)
        {
            await connection.ExecuteAsync(Upsert, new {setting.Key, setting.Value}, transaction);
        }

        transaction.Commit();
    }

    private static Dictionary<string, string> Load()
    {
        InitializeTable();
        return LoadSettings();
    }

    private static void InitializeTable()
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        var exists = connection.ExecuteScalar<int>(TableExists);
        if (exists == 1) return;

        using var transaction = connection.BeginTransaction();

        connection.Execute(CreateTable, transaction);

        foreach (var setting in Defaults)
        {
            connection.Execute(Upsert, new {setting.Key, setting.Value}, transaction);
        }

        transaction.Commit();
    }

    private static Dictionary<string, string> LoadSettings()
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        var settings = connection.Query("SELECT * FROM Setting");

        var cache = new Dictionary<string, string>();

        foreach (var setting in settings)
        {
            cache.TryAdd(setting.Key, setting.Value);
        }

        return cache;
    }
}