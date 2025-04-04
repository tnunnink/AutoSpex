using Ardalis.SmartEnum.Dapper;
using AutoSpex.Engine;
using Dapper;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using NLog.Extensions.Logging;

namespace AutoSpex.Persistence;

public static class ServiceExtensions
{
    public static void RegisterPersistence(this IServiceCollection services)
    {
        services.AddLogging(c => c.AddNLog());

        services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(ServiceExtensions).Assembly)
            //.AddOpenBehavior(typeof(LoggingBehavior<,>))
        );

        /*services.For(typeof(IRequestExceptionHandler<,,>)).Use(typeof(ExceptionBehavior<,,>));*/

        services.AddTransient<IConnectionManager>(_ => ConnectionManager.Default);

        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));
        SqlMapper.RemoveTypeMap(typeof(object));
        SqlMapper.AddTypeHandler(new SqlGuidHandler());
        SqlMapper.AddTypeHandler(new SqlTypeHandler());
        SqlMapper.AddTypeHandler(new SqlUriHandler());
        SqlMapper.AddTypeHandler(new SqlObjectHandler());
        SqlMapper.AddTypeHandler(new SqlSpecHandler());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<Element, string>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<Operation, string>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<NodeType, int>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<TypeGroup, int>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<ResultState, int>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<Negation, bool>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<Match, int>());
    }
}