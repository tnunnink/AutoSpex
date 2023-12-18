using System;
using Ardalis.SmartEnum.Dapper;
using AutoSpex.Client.Behaviors;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Features.Projects.Services;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Client.Windows;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.Messaging;
using Dapper;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Lamar;
using MediatR.Pipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using IDialogService = HanumanInstitute.MvvmDialogs.IDialogService;

namespace AutoSpex.Client;

public class Bootstrapper
{
    public static IContainer Build(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("App")
                               ?? throw new InvalidOperationException("No connection string found");

        var registry = new ServiceRegistry();
        registry.Use<ShellView>().Singleton();
        registry.Use<LauncherView>().Singleton();
        registry.For<TopLevel>().Use<ShellView>();
        registry.For<Window>().Use<ShellView>();
        registry.For<IMessenger>().Use(_ => WeakReferenceMessenger.Default).Singleton();
        registry.For<INotificationService>().Use(c => new NotificationService(c.GetInstance<TopLevel>)).Singleton();
        registry.For<ISettingsManager>().Use(_ => SettingsManager.Load(connectionString)).Singleton();
        registry.For<IDataStoreProvider>().Use<DataStoreProvider>().Singleton();
        registry.For<IProjectMigrator>().Use<ProjectMigrator>().Transient();
        registry.For<IClipboard>().Use(c => c.GetInstance<Window>().Clipboard!).Singleton();
        
        registry.For<IDialogService>().Use(c => new DialogService(
                new DialogManager(
                    viewLocator: new ViewLocator(),
                    dialogFactory: new DialogFactory().AddDialogHost())
                {
                    AllowConcurrentDialogs = true
                },
                viewModelFactory: c.GetInstance))
            .Singleton();

        registry.Scan(s =>
        {
            s.AddAllTypesOf<ViewModelBase>();
            s.WithDefaultConventions();
        });

        registry.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<Bootstrapper>()
                .AddOpenBehavior(typeof(LoggingBehavior<,>))
                .AddOpenBehavior(typeof(NotificationBehavior<,>)));

        registry.For(typeof(IRequestExceptionHandler<,,>)).Use(typeof(ExceptionBehavior<,,>));
        
        SqlMapper.AddTypeHandler(new SqlGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<NodeType, int>());

        registry.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(Constant.AppConnection)
                .ScanIn(typeof(Bootstrapper).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddSerilog())
            .Configure<RunnerOptions>(opt => { opt.Tags = new[] { "App" }; });

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