using Microsoft.Extensions.DependencyInjection;

namespace AutoSpex.Persistence.Tests;

public sealed class TestContext : IDisposable
{
    private readonly ServiceProvider _provider;
    private const string AppDb = "../spex.db";

    public TestContext()
    {
        var services = new ServiceCollection();
        services.RegisterPersistence();
        _provider = services.BuildServiceProvider();
    }

    public T Resolve<T>() where T : notnull => _provider.GetRequiredService<T>();

    public void Dispose()
    {
        _provider.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        if (File.Exists(AppDb)) File.Delete(AppDb);
    }
}