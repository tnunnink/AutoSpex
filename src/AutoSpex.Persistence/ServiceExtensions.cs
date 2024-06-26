﻿using System.Text.Json;
using Ardalis.SmartEnum.Dapper;
using AutoSpex.Engine;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace AutoSpex.Persistence;

public static class ServiceExtensions
{
    public static void RegisterPersistence(this IServiceCollection services)
    {
        services.AddMediatR(c =>
            c.RegisterServicesFromAssembly(typeof(ServiceExtensions).Assembly)
                .AddOpenBehavior(typeof(NotificationBehavior<,>))
                .AddOpenBehavior(typeof(ChangeLogBehavior<,>))
            );

        services.AddSingleton<IConnectionManager>(_ => ConnectionManager.Default);

        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonSpecConverter());
        options.Converters.Add(new JsonCriterionConverter());
        options.Converters.Add(new JsonVariableConverter());
        options.Converters.Add(new JsonArgumentConverter());
        options.Converters.Add(new JsonTypeConverter());
        services.AddSingleton(options);

        SqlMapper.AddTypeHandler(new SqlGuidHandler());
        SqlMapper.AddTypeHandler(new SqlTypeHandler());
        SqlMapper.AddTypeHandler(new SqlUriHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<Element, string>());
        SqlMapper.AddTypeHandler(new SmartEnumByValueTypeHandler<Operation, string>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<NodeType, int>());
        SqlMapper.AddTypeHandler(new SmartEnumByNameTypeHandler<TypeGroup, int>());
    }
}