using System;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Messaging;
using Lamar;

namespace AutoSpex.Client;

public static class Registrar
{
    private static Container? _container;

    public static T Resolve<T>() where T : class
    {
        if (_container is null)
            throw new InvalidOperationException(
                "Container is not initialized. Call build providing configuration before resolving.");

        return _container.GetInstance<T>();
    }

    public static void Build()
    {
        var registry = new ServiceRegistry();

        registry.RegisterShell();
        registry.RegisterPersistence();
        registry.RegisterMessenger();
        registry.RegisterNavigator();
        registry.RegisterNotifier();
        registry.RegisterPrompter();
        registry.RegisterPages();

        _container = new Container(registry);
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

    internal static void RegisterPages(this ServiceRegistry registry)
    {
        registry.Scan(s =>
        {
            s.AddAllTypesOf<PageViewModel>();
            s.WithDefaultConventions();
        });
    }
}