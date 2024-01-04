using System;
using Ardalis.SmartEnum.Dapper;
using AutoSpex.Client.Behaviors;
using AutoSpex.Client.Features.Criteria;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Features.Projects.Services;
using AutoSpex.Client.Services;
using AutoSpex.Client.Services.Mocks;
using AutoSpex.Client.Services.Options;
using AutoSpex.Client.Shared;
using AutoSpex.Client.Windows;
using AutoSpex.Engine;
using AutoSpex.Engine.Operations;
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

public class Container
{
    private static IContainer? _container;
    
    public static T Resolve<T>()
    {
        if (_container is null)
            throw new InvalidOperationException(
                "Container is not initialized. Call build providing configuration before resolving.");

        return _container.GetInstance<T>();
    }
    
    public static void Build(IConfiguration configuration)
    {
        var registry = new ServiceRegistry();
        
        registry.Use<ShellView>().Singleton();
        registry.Use<LauncherView>().Singleton();
        registry.RegisterMessenger();
        registry.RegisterMediatr();
        registry.RegisterAppDatabase();
        registry.RegisterProjectDatabase();
        registry.RegisterNotificationService(configuration);
        registry.RegisterClipboardService(configuration);
        registry.RegisterMigrationService();
        registry.RegisterDialogService();
        registry.RegisterFluentMigration();
        registry.RegisterViewModels();
        registry.RegisterSqlMappers();

        var container = new Lamar.Container(registry);

        InitializeAppStorage(container);

        _container = container;
    }

    public static void Dispose()
    {
        _container?.Dispose();
    }
    
    public static async Task DisposeAsync()
    {
        if (_container is not null) await _container.DisposeAsync();
    }

    private static void InitializeAppStorage(IServiceContext container)
    {
        var migrator = container.GetInstance<IMigrationRunner>();
        migrator.MigrateUp();
    }
}

internal static class RegistrationExtensions
{
    internal static void RegisterMessenger(this ServiceRegistry registry)
    {
        registry.For<IMessenger>().Use(_ => WeakReferenceMessenger.Default).Singleton();
    }
    
    internal static void RegisterMediatr(this ServiceRegistry registry)
    {
        registry.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<Container>()
                .AddOpenBehavior(typeof(LoggingBehavior<,>))
                .AddOpenBehavior(typeof(NotificationBehavior<,>)));

        registry.For(typeof(IRequestExceptionHandler<,,>)).Use(typeof(ExceptionBehavior<,,>));
    }
    
    internal static void RegisterAppDatabase(this ServiceRegistry registry)
    {
        registry.For<AppDatabase>().Use(_ => new AppDatabase(() => "app.db")).Transient();
    }
    
    internal static void RegisterProjectDatabase(this ServiceRegistry registry)
    {
        registry.For<ProjectDatabase>()
            .Use(_ => new ProjectDatabase(() => Settings.App.OpenProject))
            .Transient();
    }
    
    internal static void RegisterMigrationService(this ServiceRegistry registry)
    {
        registry.For<IProjectMigrator>().Use<ProjectMigrator>().Transient();
    }
    
    internal static void RegisterFluentMigration(this ServiceRegistry registry)
    {
        registry.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(Constant.AppConnection)
                .ScanIn(typeof(Container).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddSerilog())
            .Configure<RunnerOptions>(opt => { opt.Tags = new[] {"App"}; });
    }

    internal static void RegisterDialogService(this ServiceRegistry registry)
    {
        registry.For<IDialogService>().Use(c => new DialogService(
                new DialogManager(
                    viewLocator: new ViewLocator(),
                    dialogFactory: new DialogFactory().AddDialogHost())
                {
                    AllowConcurrentDialogs = true
                },
                viewModelFactory: c.GetInstance))
            .Singleton();
    }

    internal static void RegisterNotificationService(this ServiceRegistry registry, IConfiguration config)
    {
        var options = config.GetSection(nameof(ServiceOptions)).Get<ServiceOptions>();

        if (options.MockNotificationService)
        {
            registry.For<INotificationService>().Use<MockNotificationService>().Singleton();
            return;
        }

        registry.For<INotificationService>().Use(_ => new NotificationService(() => App.MainWindow)).Singleton();
    }

    internal static void RegisterClipboardService(this ServiceRegistry registry, IConfiguration config)
    {
        var options = config.GetSection(nameof(ServiceOptions)).Get<ServiceOptions>();

        if (options.MockClipboardService)
        {
            registry.For<IClipboardService>().Use<MockClipboardService>().Transient();
            return;
        }

        registry.For<IClipboardService>().Use(_ => new ClipboardService(App.MainWindow)).Transient();
    }
    
    internal static void RegisterViewModels(this ServiceRegistry registry)
    {
        registry.Scan(s =>
        {
            s.AddAllTypesOf<ViewModelBase>();
            s.WithDefaultConventions();
        });
    }
    
    // ReSharper disable once UnusedParameter.Global just following extension pattern anyway.
    internal static void RegisterSqlMappers(this ServiceRegistry registry)
    {
        SqlMapper.AddTypeHandler(new SqlGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<Element, string>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<Operation, string>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<NodeType, int>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<Feature, int>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<CriterionUsage, int>());
    }
}