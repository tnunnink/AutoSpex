﻿using Microsoft.Extensions.DependencyInjection;

namespace AutoSpex.Persistence.Tests;

public sealed class TestContext : IDisposable
{
    private readonly ServiceProvider _provider;
    private const string AppDb = "app.db";
    private const string ProjectDb = "project.db";

    public TestContext(bool register = true)
    {
        var services = new ServiceCollection();

        services.RegisterPersistence();

        _provider = services.BuildServiceProvider();

        AppPath = Path.Combine(Directory.GetCurrentDirectory(), AppDb);
        ProjectPath = Path.Combine(Directory.GetCurrentDirectory(), ProjectDb);

        if (!register) return;
        var manager = _provider.GetRequiredService<IConnectionManager>();
        manager.Register(Database.App, AppPath);
        manager.Register(Database.Project, ProjectPath);
    }

    public readonly string AppPath;

    public readonly string ProjectPath;

    public T Resolve<T>() where T : notnull => _provider.GetRequiredService<T>();

    public void Dispose()
    {
        _provider.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        if (File.Exists(AppPath)) File.Delete(AppPath);
        if (File.Exists(ProjectPath)) File.Delete(ProjectPath);
    }
}