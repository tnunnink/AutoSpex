using System;
using AutoSpex.Client.Behaviors;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Messaging;
using Lamar;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace AutoSpex.Client;

public class Container
{
    private static IContainer? _container;

    public static T Resolve<T>() where T : class
    {
        if (_container is null)
            throw new InvalidOperationException(
                "Container is not initialized. Call build providing configuration before resolving.");

        return _container.GetInstance<T>();
    }

    public static object Resolve(Type type)
    {
        if (_container is null)
            throw new InvalidOperationException(
                "Container is not initialized. Call build providing configuration before resolving.");

        return _container.GetInstance(type);
    }

    public static T? TryResolve<T>() where T : class
    {
        return _container?.TryGetInstance<T>();
    }

    public static void Build()
    {
        var registry = new ServiceRegistry();

        registry.RegisterShell();
        registry.RegisterMediatr();
        registry.RegisterPersistence();
        registry.RegisterMessenger();
        registry.RegisterNavigator();
        registry.RegisterNotifier();
        registry.RegisterPrompter();
        registry.RegisterLauncher();
        registry.RegisterPages();

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
    internal static void RegisterShell(this ServiceRegistry registry)
    {
        registry.For<Shell>().Use<Shell>().Singleton();
    }

    internal static void RegisterMediatr(this ServiceRegistry registry)
    {
        registry.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<Container>()
                .AddOpenBehavior(typeof(LoggingBehavior<,>)));

        registry.For(typeof(IRequestExceptionHandler<,,>)).Use(typeof(ExceptionBehavior<,,>));
    }

    internal static void RegisterMessenger(this ServiceRegistry registry)
    {
        registry.For<IMessenger>().Use(_ => WeakReferenceMessenger.Default).Singleton();
    }

    internal static void RegisterNavigator(this ServiceRegistry registry)
    {
        registry.For<Navigator>().Use<Navigator>().Singleton();
    }

    internal static void RegisterNotifier(this ServiceRegistry registry)
    {
        registry.For<Notifier>().Use<Notifier>().Singleton();
    }
    
    internal static void RegisterPrompter(this ServiceRegistry registry)
    {
        registry.For<Prompter>().Use<Prompter>().Singleton();
    }

    internal static void RegisterLauncher(this ServiceRegistry registry)
    {
        registry.For<Launcher>().Use<Launcher>().Transient();
    }

    internal static void RegisterPages(this ServiceRegistry registry)
    {
        registry.Scan(s =>
        {
            s.AddAllTypesOf<PageViewModel>();
            s.WithDefaultConventions();
        });
    }
}