using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using ActiproSoftware.UI.Avalonia.Controls;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Client.Windows;
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
    private readonly ISettingsManager _settings;

    public App()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        var config = builder.Build();
        _container = Bootstrapper.Build(config);
        _settings = _container.GetInstance<ISettingsManager>();

        PropertyChanged += (_, e) =>
        {
            if (e.Property != RequestedThemeVariantProperty) return;
            var theme = e.NewValue;
            _settings.Set(Setting.Theme, theme);
            _settings.Save();
        };
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

        var theme = _settings.Find(Setting.Theme);

        RequestedThemeVariant = theme is not null
            ? theme == "Light" ? ThemeVariant.Light
            : theme == "Dark" ? ThemeVariant.Dark
            : ThemeVariant.Default
            : ThemeVariant.Default;
    }

    public static App Instance => (App)Current!;

    public static IContainer Container => ((App)Current!)._container;

    public static Window MainWindow =>
        ((IClassicDesktopStyleApplicationLifetime)Current!.ApplicationLifetime!).MainWindow!;

    public static ISettingsManager Settings => ((App)Current!)._settings;

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            //Tells Actipro to show all prompts as an overlay by default
            UserPromptBuilder.DefaultDisplayMode = UserPromptDisplayMode.Overlay;

            desktop.MainWindow = _container.GetInstance<LauncherView>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void OpenShell()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;

        var current = desktop.MainWindow;
        current?.Hide();

        desktop.MainWindow = _container.GetInstance<ShellView>();
        desktop.MainWindow.Activate();
        desktop.MainWindow.Show();

        current?.Close();
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