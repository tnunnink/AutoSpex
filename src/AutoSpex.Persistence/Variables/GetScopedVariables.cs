using AutoSpex.Engine;
using Dapper;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetScopedVariables(Guid NodeId) : IRequest<IEnumerable<Variable>>;

[UsedImplicitly]
internal class GetScopedVariablesHandler(IConnectionManager manager)
    : IRequestHandler<GetScopedVariables, IEnumerable<Variable>>
{
    private const string GetInheritedVariables =
        """
        WITH Tree AS (
            SELECT NodeId, ParentId
            FROM Node
            WHERE NodeId = @NodeId
            UNION ALL
            SELECT n.NodeId, n.ParentId
            FROM Node n
                     INNER JOIN Tree t ON t.ParentId = n.NodeId)

        SELECT [VariableId], [Name], [Group], [Value]
        FROM Tree t
        JOIN Variable v ON v.NodeId = t.NodeId
        """;

    public async Task<IEnumerable<Variable>> Handle(GetScopedVariables request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        
        var inherited = await connection.QueryAsync<Variable>(GetInheritedVariables, new { request.NodeId });

        var scoped = new Dictionary<string, Variable>();
        
        foreach (var variable in inherited)
            scoped.TryAdd(variable.Name, variable);

        return scoped.Select(s => s.Value);
    }
}