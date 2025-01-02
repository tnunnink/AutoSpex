using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Action = AutoSpex.Engine.Action;

namespace AutoSpex.Persistence;

[PublicAPI]
public record AddSuppression(Guid SourceId, Action Action) : ICommandRequest<Result>
{
    public IEnumerable<Change> GetChanges()
    {
        yield return Change.For<AddSuppression>(SourceId, ChangeType.Updated, $"Added Suppression {Action.Reason}");
    }
}

[UsedImplicitly]
internal class AddSuppressionHandler(IConnectionManager manager) : IRequestHandler<AddSuppression, Result>
{
    private const string AddSuppression =
        """
        INSERT INTO Action (SourceId, NodeId, Type, Reason) 
        VALUES (@SourceId, @NodeId, @Type, @Reason)
        ON CONFLICT DO UPDATE SET Reason = @Reason;
        """;

    public async Task<Result> Handle(AddSuppression request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        await connection.ExecuteAsync(AddSuppression, new
            {
                request.SourceId,
                request.Action.NodeId,
                request.Action.Type,
                request.Action.Reason
            }
        );
        return Result.Ok();
    }
}