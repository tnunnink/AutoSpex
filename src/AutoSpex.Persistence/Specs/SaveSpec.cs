using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SaveSpec(Guid NodeId, Spec Spec) : IDbCommand<Result>;

[UsedImplicitly]
internal class SaveSpecHandler(IConnectionManager manager) : IRequestHandler<SaveSpec, Result>
{
    private const string UpsertSpec =
        "INSERT INTO Spec(NodeId, Specification) VALUES (@NodeId, @Specification) " +
        "ON CONFLICT DO UPDATE SET Specification = @Specification;";

    public async Task<Result> Handle(SaveSpec request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var record = new {request.NodeId, Specification = request.Spec.Serialize()};
        await connection.ExecuteAsync(UpsertSpec, record);
        return Result.Ok();
    }
}