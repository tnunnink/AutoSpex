using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace L5Spex.Persistence.Tests;

public static class Testing
{
    private const string TestDatabase = "Data Source=Test.db;";
    private static readonly IServiceScopeFactory ScopeFactory;
    
    static Testing()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(SqliteConnectionProvider).Assembly);
        });
        services.AddSingleton<IConnectionProvider>(_ => new SqliteConnectionProvider(TestDatabase));
        services.AddSingleton<IMigrationProvider>(_ => new SqliteMigrationProvider(TestDatabase));
        var provider = services.BuildServiceProvider();
        ScopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
    }

    private static void ExecuteScope(Action<IServiceProvider> action)
    {
        using var scope = ScopeFactory.CreateScope();
        action(scope.ServiceProvider);
    }

    private static async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = ScopeFactory.CreateScope();
        return await action(scope.ServiceProvider);
    }
    
    public static void CreateDatabase()
    {
        ExecuteScope(sp => sp.GetRequiredService<IMigrationProvider>().Migrate());
    }
    
    public static void DeleteDatabase()
    {
        File.Delete(TestDatabase);
    }
    
    public static Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        return ExecuteScopeAsync(sp => sp.GetRequiredService<IMediator>().Send(request));
    }
}