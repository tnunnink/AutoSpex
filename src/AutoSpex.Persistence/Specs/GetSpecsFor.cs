using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetSpecsFor(Guid NodeId) : IDbQuery<Result<IEnumerable<Spec>>>;

[UsedImplicitly]
internal class GetSpecsForHandler(IConnectionManager manager) : IRequestHandler<GetSpecsFor, Result<IEnumerable<Spec>>>
{
    private const string GetSpecs = """
                                    WITH Tree AS
                                             (SELECT NodeId, ParentId, NodeType, Name
                                              FROM Node
                                              WHERE NodeId = @NodeId
                                    
                                              UNION ALL
                                    
                                              SELECT n.NodeId, n.ParentId, n.NodeType, n.Name
                                              FROM Node n
                                                       INNER JOIN Tree t ON n.ParentId = t.NodeId)
                                    SELECT s.SpecId, s.Specification
                                    FROM Tree t
                                    JOIN Spec s on s.SpecId = t.NodeId;
                                    """;
    
    public async Task<Result<IEnumerable<Spec>>> Handle(GetSpecsFor request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        var results = await connection.QueryAsync(GetSpecs, new {request.NodeId});

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