using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using ActiproSoftware.UI.Avalonia.Controls;
using ActiproSoftware.UI.Avalonia.Media;
using AutoSpex.Client.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace AutoSpex.Client;

[UsedImplicitly]
public sealed class App : Application, IDisposable, IAsyncDisposable
{
    /*private readonly ISettingsManager _settings;*/

    public App()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        var config = builder.Build();
        Container.Build(config);

        PropertyChanged += (_, e) =>
        {
            if (e.Property != RequestedThemeVariantProperty) return;
            var theme = e.NewValue as ThemeVariant ?? throw new InvalidOperationException("Not a ThemeVariant");
            Settings.App.Save(s => s.Theme = theme);
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

        ImageProvider.Default.ChromaticAdaptationMode = ImageChromaticAdaptationMode.DarkThemes;
        
        RequestedThemeVariant = Settings.App.Theme;
    }

    public static App Instance => (App)Current!;

    public static Window MainWindow =>
        ((IClassicDesktopStyleApplicationLifetime)Current!.ApplicationLifetime!).MainWindow!;

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            //Tells Actipro to show all prompts as an overlay by default
            UserPromptBuilder.DefaultDisplayMode = UserPromptDisplayMode.Overlay;

            desktop.MainWindow = Container.Resolve<LauncherView>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void OpenShell()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;

        var current = desktop.MainWindow;
        current?.Hide();

        desktop.MainWindow = Container.Resolve<ShellView>();
        desktop.MainWindow.Activate();
        desktop.MainWindow.Show();

        current?.Close();
    }

    public void Dispose()
    {
        Container.Dispose();
        Log.CloseAndFlush();
    }

    public async ValueTask DisposeAsync()
    {
        await Container.DisposeAsync();
        await Log.CloseAndFlushAsync();
    }
}