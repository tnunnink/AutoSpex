using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetFullNode(Guid NodeId) : IDbQuery<Result<Node>>;

[UsedImplicitly]
internal class GetFullNodeHandler(IConnectionManager manager) : IRequestHandler<GetFullNode, Result<Node>>
{
    private const string Query =
        "SELECT NodeId, ParentId, NodeType, Name, Depth, Ordinal FROM Node WHERE NodeId = @NodeId;" +
        "SELECT Specification FROM Spec WHERE NodeId = @NodeId;" +
        "SELECT VariableId, Name, Value FROM Variable WHERE NodeId = @NodeId;";

    public async Task<Result<Node>> Handle(GetFullNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        await using var reader = await connection.QueryMultipleAsync(Query, new {request.NodeId});

        var node = (await reader.ReadAsync<Node>()).SingleOrDefault();
        if (node is null)
            return Result.Fail<Node>($"No node found with id '{request.NodeId}'");

        //We have to read no matter what to advance the reader.
        var specification = (await reader.ReadAsync<string>()).SingleOrDefault();
        if (node.NodeType == NodeType.Spec)
        {
            if (specification is null)
                return Result.Fail<Node>($"No specification data was found for node '{request.NodeId}'");
            
            var spec = Spec.Deserialize(specification);
            node.Configure(spec);
        }
        
        var variables = await reader.ReadAsync<Variable>();
        node.AddVariables(variables);
        
        return Result.Ok(node);
    }
}