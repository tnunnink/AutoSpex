using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using ActiproSoftware.UI.Avalonia.Controls;
using AutoSpex.Client.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using JetBrains.Annotations;
using Lamar;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace AutoSpex.Client;

[UsedImplicitly]
public sealed class App : Application, IDisposable, IAsyncDisposable
{
    private readonly IContainer _container;
    private readonly IConfiguration _configuration;
    private readonly ISettingsManager _settings;

    public App()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        _configuration = builder.Build();
        _container = Bootstrapper.Build(_configuration);
        _settings = _container.GetInstance<ISettingsManager>();
    }

    private static void InitializeLogger(IConfiguration configuration)
    {
        Serilog.Debugging.SelfLog.Enable(Console.WriteLine);
        
        var connectionString = configuration.GetConnectionString("LogConnection");

        var connStringBuilder = new SQLiteConnectionStringBuilder(connectionString);
        var dbPath = connStringBuilder.DataSource;
        
        if (!File.Exists(dbPath))
        {
            using var connection = new SQLiteConnection(connectionString);
            connection.Open();
        }
        
        Log.Logger = new LoggerConfiguration()
            .WriteTo.SQLite(configuration.GetConnectionString("LogConnection"),
                tableName: "Log",
                LogEventLevel.Information,
                storeTimestampInUtc: true,
                batchSize: 1)
            .CreateLogger();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        RequestedThemeVariant = ThemeVariant.Light;
    }

    public static IContainer Container => ((App) Current!)._container;
    
    public static ISettingsManager Settings => ((App) Current!)._settings;

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            //Tells Actipro to show all prompts as an overlay by default
            UserPromptBuilder.DefaultDisplayMode = UserPromptDisplayMode.Overlay;

            desktop.MainWindow = _container.GetInstance<Window>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void Dispose()
    {
        _settings.Dispose();
        _container.Dispose();
        Log.CloseAndFlush();
    }

    public async ValueTask DisposeAsync()
    {
        await _settings.DisposeAsync();
        await _container.DisposeAsync();
        await Log.CloseAndFlushAsync();
    }
}