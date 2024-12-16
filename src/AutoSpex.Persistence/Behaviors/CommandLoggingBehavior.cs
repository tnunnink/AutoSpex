using System.Diagnostics;
using Dapper;
using FluentResults;
using MediatR;

namespace AutoSpex.Persistence;

//Not using this at this point. Could maybe add in the future but not sure if it's really worth it.
public class CommandLoggingBehavior<TRequest, TResponse>(IConnectionManager manager)
    : IPipelineBehavior<TRequest, TResponse>
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

        using var connection = await manager.Connect(cancellationToken);
        await connection.ExecuteAsync(InsertLog, log);
        return result;
    }
}