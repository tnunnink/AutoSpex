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
    private const string GetSpecs = """
                                    SELECT SpecId, n.Name as Name, Specification 
                                    FROM Spec s
                                    JOIN Node n on n.NodeId = s.SpecId
                                    WHERE SpecId IN @Ids
                                    """;
    
    public async Task<Result<IEnumerable<Spec>>> Handle(GetSpecsIn request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        
        var results = await connection.QueryAsync(GetSpecs, new {Ids = request.SpecIds.ToList()});
        
        var specs = new List<Spec>();
        
        foreach (var result in results)
        {
            var config = Spec.Deserialize(result.Specification);
            var spec = new Spec(Node.Create(result.SpecId, NodeType.Spec, result.Name));
            spec.Update(config);
            specs.Add(spec);
        }
        
        return Result.Ok(specs.AsEnumerable());
    }
}
