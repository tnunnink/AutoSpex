﻿using System.Diagnostics;
using Dapper;
using FluentResults;
using MediatR;

namespace AutoSpex.Persistence;

public class ChangeLogBehavior<TRequest, TResponse>(IConnectionManager manager) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IDbCommand<TResponse>, IDbLoggable
    where TResponse : IResultBase
{
    private const string InsertLog =
        """
        INSERT INTO ChangeLog (ChangeId, NodeId, Command, Message, ChangedOn, ChangedBy, Machine, Duration)
        VALUES (@ChangeId, @NodeId, @Command, @Message, @ChangedOn, @ChangedBy, @Machine, @Duration)
        """;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await next();
        stopwatch.Stop();

        if (result.IsFailed) return result;

        var log = new
        {
            ChangeId = Guid.NewGuid().ToString(),
            NodeId = request.NodeId.ToString(),
            Command = typeof(TRequest).Name,
            request.Message,
            ChangedOn = DateTime.UtcNow,
            ChangedBy = Environment.UserName,
            Machine = Environment.MachineName,
            Duration = stopwatch.ElapsedMilliseconds
        };

        using var connection = await manager.Connect(Database.Project, cancellationToken);
        await connection.ExecuteAsync(InsertLog, log);
        return result;
    }
}