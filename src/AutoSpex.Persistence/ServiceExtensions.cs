using Ardalis.SmartEnum.Dapper;
using AutoSpex.Engine;
using Dapper;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace AutoSpex.Persistence;

public static class ServiceExtensions
{
    public static void RegisterPersistence(this IServiceCollection services)
    {
        services.AddMediatR(c =>
                c.RegisterServicesFromAssembly(typeof(ServiceExtensions).Assembly)
                    .AddOpenBehavior(typeof(NotificationBehavior<,>))
            /*.AddOpenBehavior(typeof(ChangeLogBehavior<,>))*/
        );

        services.AddTransient<IConnectionManager, ConnectionManager>();

        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));
        SqlMapper.RemoveTypeMap(typeof(object));
        SqlMapper.AddTypeHandler(new SqlGuidHandler());
        SqlMapper.AddTypeHandler(new SqlTypeHandler());
        SqlMapper.AddTypeHandler(new SqlUriHandler());
        SqlMapper.AddTypeHandler(new SqlObjectHandler());
        SqlMapper.AddTypeHandler(new SqlSpecHandler());
        SqlMapper.AddTypeHandler(new SqlVerificationHandler());
        SqlMapper.AddTypeHandler(new SqlL5XHandler());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<Element, string>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<Operation, string>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<NodeType, int>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<TypeGroup, int>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<ResultState, int>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<Negation, bool>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<Match, int>());

        Migrate();
    }

    /// <summary>
    /// Migrate the database once the services are registered to make the persistence layer ready for use.
    /// For whatever reason I get an exception when registering fluent migrator with Lamar. That is why I am using
    /// the build in MS DI provider here and just calling this upon registration.
    /// </summary>
    private static void Migrate()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();
        var runner = provider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(ConnectionManager.ConnectionString)
                .ScanIn(typeof(ConnectionManager).Assembly).For.Migrations());

        return services.BuildServiceProvider();
    }
}