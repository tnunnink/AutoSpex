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
    private const string GetSpec = "SELECT Specification FROM Spec WHERE SpecId = @SpecId;";

    public async Task<Result<Spec>> Handle(GetSpec request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        var specification = await connection.QuerySingleOrDefaultAsync<string>(GetSpec, new {SpecId = request.NodeId});

        if (specification is null)
            return Result.Fail<Spec>($"No specification data was found for node '{request.NodeId}'.");

        var config = Spec.Deserialize(specification);
        var spec = new Spec(request.NodeId);
        spec.Configure(config);
        
        return Result.Ok(spec);
    }
}