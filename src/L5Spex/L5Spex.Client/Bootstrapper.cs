using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FluentMigrator.Runner;
using L5Spex.Client.Pipelines;
using L5Spex.Client.Services;
using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace L5Spex.Client;

public class Bootstrapper
{
    public static IContainer Build(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("No connection string found");

        var registry = new ServiceRegistry();
        registry.Use<Shell>().Scoped().For<Window>().For<TopLevel>();
        registry.For<IStorageProvider>().Use(c => c.GetInstance<Window>().StorageProvider).Scoped();
        registry.For<INotificationManager>().Use(ctx => new WindowNotificationManager(ctx.GetInstance<TopLevel>()))
            .Scoped();
        registry.For<IMessenger>().Use(_ => WeakReferenceMessenger.Default).Scoped();
        registry.For<IDatabaseProvider>().Use(_ => new SqliteDatabaseProvider(connectionString)).Transient();

        //Types implementing observable are typically view models. This way I don't need to register each one.
        registry.Scan(s =>
        {
            s.AddAllTypesOf<ObservableObject>();
            s.AddAllTypesOf<ObservableValidator>();
            s.AddAllTypesOf<ObservableRecipient>();
            s.WithDefaultConventions();
        });

        registry.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<Bootstrapper>()
                .AddOpenBehavior(typeof(LoggingPipeline<,>)));
        
        registry.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(Bootstrapper).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        var container = new Container(registry);

        var migrator = container.GetInstance<IMigrationRunner>();
        migrator.MigrateUp();

        return container;
    }
}