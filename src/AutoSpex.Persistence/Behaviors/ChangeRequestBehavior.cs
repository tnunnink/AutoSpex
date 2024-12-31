using Dapper;
using FluentResults;
using MediatR;

namespace AutoSpex.Persistence;

public class ChangeRequestBehavior<TRequest, TResponse>(IConnectionManager manager)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommandRequest<TResponse>, IChangeRequest
    where TResponse : IResultBase
{
    private const string InsertChange =
        """
        INSERT INTO Change (ChangeId, EntityId, Request, ChangeType, ChangedOn, ChangedBy, Message)
        VALUES (@ChangeId, @EntityId, @Request, @ChangeType, @ChangedOn, @ChangedBy, @Message)
        """;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var result = await next();
        if (result.IsFailed) return result;

        using var connection = await manager.Connect(cancellationToken);
        var changes = request.GetChanges();
        await connection.ExecuteAsync(InsertChange, changes);

        return result;
    }
}