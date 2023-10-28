using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using FluentMigrator.Runner;
using L5Spex.Client.Services;
using L5Spex.Client.ViewModels;
using L5Spex.Client.Views;
using Microsoft.Extensions.DependencyInjection;

namespace L5Spex.Client;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    /// <summary>
    /// Static application instance access. Used by views to resolve services.
    /// </summary>
    public static App Local => Current as App;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider Services { get; private set; }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();
            
            services.AddSingleton<IConnectionProvider>(_ => new ConnectionProvider(DatabaseConnection));
            services.AddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);
            services.AddSingleton<IFileService>(_ => new FileService(desktop));
            services.AddViewModels();
            services.AddMigrator();

            Services = services.BuildServiceProvider();
            
            //Migrate the database
            var migrator = Services.GetRequiredService<IMigrationRunner>();
            migrator.MigrateUp();
            
            desktop.MainWindow = new ShellView
            {
                DataContext = new ShellViewModel()
            };
        }
        
        base.OnFrameworkInitializationCompleted();
    }
    
    public const string RepositoryUrl = @"https://github.com/tnunnink/L5Spex";

    public const string ReadMeUrl = @"https://github.com/tnunnink/L5Spex/blob/main/README.md";

    public const string IssuesUrl = @"https://github.com/tnunnink/L5Spex/issues";
    
    public const string DatabaseConnection = "Data Source=Spex.db;";
}

public static class RegistrationExtensions
{
    public static void AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<ShellViewModel>();
        //services.AddTransient<SourcesViewModel>();
        //todo register remaining view models
    }

    public static void AddMigrator(this IServiceCollection services)
    {
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(App.DatabaseConnection)
                .ScanIn(typeof(App).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());
    }
}