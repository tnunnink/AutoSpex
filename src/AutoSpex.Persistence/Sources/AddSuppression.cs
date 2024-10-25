using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record AddSuppression(Guid SourceId, Suppression Suppression) : IDbCommand<Result>;

[UsedImplicitly]
internal class AddSuppressionHandler(IConnectionManager manager) : IRequestHandler<AddSuppression, Result>
{
    private const string AddSuppression =
        """
        INSERT INTO Suppression (SourceId, NodeId, Reason) 
        VALUES (@SourceId, @NodeId, @Reason)
        ON CONFLICT DO UPDATE SET Reason = @Reason;
        """;

    public async Task<Result> Handle(AddSuppression request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        await connection.ExecuteAsync(AddSuppression, new
            {
                request.SourceId,
                request.Suppression.NodeId,
                request.Suppression.Reason
            }
        );
        return Result.Ok();
    }
}