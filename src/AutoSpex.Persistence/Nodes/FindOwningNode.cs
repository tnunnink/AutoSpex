using AutoSpex.Engine;
using Dapper;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record FindOwningNode(Guid Id) : IRequest<Node>;

[UsedImplicitly]
internal class FindOwningNodeHandler(IConnectionManager manager) : IRequestHandler<FindOwningNode, Node>
{
    private const string GetNodeId =
        """
        SELECT NodeId FROM Spec WHERE SpecId = @Id
        UNION
        SELECT NodeId FROM Variable WHERE VariableId = @Id;
        """;

    private const string GetNode =
        "SELECT NodeId, ParentId, Type, Name FROM Node WHERE NodeId = @NodeId";

    public async Task<Node> Handle(FindOwningNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var nodeId = await connection.QuerySingleAsync<Guid>(GetNodeId, new { request.Id });

        return await connection.QuerySingleAsync<Node>(GetNode, new { NodeId = nodeId });
    }
}