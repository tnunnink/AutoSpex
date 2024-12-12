using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadSpec(Guid NodeId) : IDbQuery<Result<Spec>>;

[UsedImplicitly]
internal class LoadSpecHandler(IConnectionManager manager) : IRequestHandler<LoadSpec, Result<Spec>>
{
    private const string LoadSpec = "SELECT Config FROM Spec WHERE NodeId = @NodeId";

    public async Task<Result<Spec>> Handle(LoadSpec request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var spec = await connection.QuerySingleOrDefaultAsync<Spec>(LoadSpec, new { request.NodeId });
        return spec is not null ? Result.Ok(spec) : Result.Fail($"Spec not found: {request.NodeId}");
    }
}