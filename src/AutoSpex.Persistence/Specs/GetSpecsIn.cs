using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetSpecsIn(IEnumerable<Guid> SpecIds) : IDbQuery<Result<IEnumerable<Spec>>>;

[UsedImplicitly]
internal class GetSpecsInHandler(IConnectionManager manager) : IRequestHandler<GetSpecsIn, Result<IEnumerable<Spec>>>
{
    private const string GetSpecs = "SELECT SpecId, Specification FROM Spec WHERE SpecId IN @Ids";
    
    public async Task<Result<IEnumerable<Spec>>> Handle(GetSpecsIn request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        
        var results = await connection.QueryAsync(GetSpecs, new {Ids = request.SpecIds.ToList()});
        
        var specs = new List<Spec>();
        
        foreach (var result in results)
        {
            var config = Spec.Deserialize(result.Specification);
            var spec = new Spec(result.SpecId);
            spec.Configure(config);
            specs.Add(spec);
        }
        
        return Result.Ok(specs.AsEnumerable());
    }
}
