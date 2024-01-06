using System;
using AutoSpex.Client.Behaviors;
using AutoSpex.Client.Services;
using AutoSpex.Client.Services.Mocks;
using AutoSpex.Client.Services.Options;
using AutoSpex.Client.Shared;
using AutoSpex.Client.Windows;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Lamar;
using MediatR.Pipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        registry.RegisterPersistence();
        registry.RegisterNotificationService(configuration);
        registry.RegisterClipboardService(configuration);
        registry.RegisterDialogService();
        registry.RegisterViewModels();

        var container = new Lamar.Container(registry);

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
                .AddOpenBehavior(typeof(LoggingBehavior<,>)));

        registry.For(typeof(IRequestExceptionHandler<,,>)).Use(typeof(ExceptionBehavior<,,>));
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
}