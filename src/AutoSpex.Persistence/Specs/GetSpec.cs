using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetSpec(Guid NodeId) : IDbQuery<Result<Spec>>;

[UsedImplicitly]
internal class GetSpecHandler(IConnectionManager manager) : IRequestHandler<GetSpec, Result<Spec>>
{
    private const string Query = "SELECT Specification FROM Spec WHERE NodeId = @NodeId;";

    public async Task<Result<Spec>> Handle(GetSpec request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        var specification = await connection.QuerySingleOrDefaultAsync<string>(Query, new {request.NodeId});
        
        if (specification is null)
            return Result.Fail<Spec>($"No specification data was found for node '{request.NodeId}'");
        
        var spec = Spec.Deserialize(specification);
        return Result.Ok(spec);
    }
}