using System.Data;
using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadNode(Node Node) : IDbQuery<Result<Node>>;

[UsedImplicitly]
internal class LoadNodeHandler(IConnectionManager manager) : IRequestHandler<LoadNode, Result<Node>>
{
    private const string GetVariables = "SELECT Name, Value FROM Variable where NodeId = @NodeId";
    private const string GetSpec = "SELECT Specification FROM Spec where NodeId = @NodeId";

    public async Task<Result<Node>> Handle(LoadNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var node = request.Node;
        await LoadVariables(connection, node);
        await LoadSpecification(connection, node);
        return Result.Ok(node);
    }

    private static async Task LoadVariables(IDbConnection connection, Node node)
    {
        var variables = await connection.QueryAsync<Variable>(GetVariables, new {node.NodeId});
        
        foreach (var variable in variables)
        {
            node.AddVariable(variable);
        }
    }

    private static async Task LoadSpecification(IDbConnection connection, Node node)
    {
        var specification = await connection.QuerySingleOrDefaultAsync<string>(GetSpec, new {node.NodeId});
        if (specification is not null)
        {
            var spec = Spec.Deserialize(specification);
            node.Configure(spec);
        }
    }
}