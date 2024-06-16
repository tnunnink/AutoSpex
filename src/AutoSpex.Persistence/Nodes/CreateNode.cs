using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

/// <summary>
/// Inserts the provided node, and additionally inserts a default row into Spec, Source, or Run depending on the node
/// type.
/// </summary>
/// <param name="Node">The node to create.</param>
[PublicAPI]
public record CreateNode(Node Node, NodeType? Type = default) : IDbCommand<Result>, IDbLoggable
{
    public Guid NodeId => Node.NodeId;
    public string Message => $"Created new {Node.Type} with name '{Node.Name}'";
}

[UsedImplicitly]
internal class CreateNodeHandler(IConnectionManager manager) : IRequestHandler<CreateNode, Result>
{
    private const string GetParent =
        "SELECT NodeId, ParentId, Type, Name FROM Node WHERE ParentId is null and Type = @Type";

    private const string InsertNode =
        "INSERT INTO Node (NodeId, ParentId, Type, Name) VALUES (@NodeId, @ParentId, @Type, @Name)";

    private const string InsertSpec = "INSERT INTO Spec (SpecId) VALUES (@NodeId)";
    private const string InsertSource = "INSERT INTO Source (SourceId) VALUES (@NodeId)";
    private const string InsertRun = "INSERT INTO Run (RunId) VALUES (@NodeId)";

    public async Task<Result> Handle(CreateNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        using var transaction = connection.BeginTransaction();

        //If we are adding to the "root" meaning parent id is empty, we need to the actual virtual root node to use
        //as the parent of this node.
        if (request.Node.ParentId == Guid.Empty)
        {
            var type = request.Type ?? request.Node.Type;

            if (type == NodeType.Container)
                return Result.Fail(
                    $"Can not determine which node type {request.Node.Name} belongs to. Specify the Type to add to proper root node.");

            var parent = await connection.QuerySingleAsync<Node>(GetParent, new { Type = type }, transaction);
            //This will set parent and parent id.
            parent.AddNode(request.Node);
        }

        await connection.ExecuteAsync(InsertNode, request.Node, transaction);

        if (request.Node.Type == NodeType.Spec)
            await connection.ExecuteAsync(InsertSpec, new { request.Node.NodeId }, transaction);
        if (request.Node.Type == NodeType.Source)
            await connection.ExecuteAsync(InsertSource, new { request.Node.NodeId }, transaction);
        if (request.Node.Type == NodeType.Run)
            await connection.ExecuteAsync(InsertRun, new { request.Node.NodeId }, transaction);

        transaction.Commit();
        return Result.Ok();
    }
}