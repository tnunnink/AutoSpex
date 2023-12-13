using System;
using AutoSpex.Client.Behaviors;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace AutoSpex.Client;

public class Bootstrapper
{
    public static IContainer Build(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("App")
                               ?? throw new InvalidOperationException("No connection string found");

        var registry = new ServiceRegistry();
        registry.Use<Shell>().Scoped().For<Window>().For<TopLevel>();
        registry.For<IMessenger>().Use(_ => WeakReferenceMessenger.Default).Singleton();
        registry.For<ISettingsManager>().Use(_ => SettingsManager.Load(connectionString)).Singleton();
        registry.For<IDataStoreProvider>().Use<DataStoreProvider>().Singleton();
        registry.For<IDialogService>().Use(c => new DialogService(c.GetInstance<Shell>)).Singleton();
        registry.For<IProjectMigrator>().Use<ProjectMigrator>().Transient();
        registry.For<IStoragePicker>().Use(c => new StoragePicker(() => c.GetInstance<Window>().StorageProvider));

        registry.Scan(s =>
        {
            s.AddAllTypesOf<ViewModelBase>();
            s.WithDefaultConventions();
        });
        
        registry.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<Bootstrapper>()
                .AddOpenBehavior(typeof(LoggingBehavior<,>))
                .AddOpenBehavior(typeof(NotificationBehavior<,>)));
        
        /*registry.For(typeof(IRequestExceptionHandler<,,>)).Use(typeof(ExceptionBehavior<,,>));*/

        registry.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(Constant.AppConnection)
                .ScanIn(typeof(Bootstrapper).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddSerilog())
            .Configure<RunnerOptions>(opt => { opt.Tags = new[] {"App"}; });

        var container = new Container(registry);
        
        InitializeAppStorage(container, connectionString);

        return container;
    }

    private static void InitializeAppStorage(IServiceContext container, string connectionString)
    {
        var migrator = container.GetInstance<IMigrationRunner>();
        migrator.MigrateUp();
        
        var settings = container.GetInstance<ISettingsManager>();
        settings.Set(Setting.AppConnection, connectionString);
        settings.Save();
    }
}