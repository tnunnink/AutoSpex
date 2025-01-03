﻿using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DeleteNodes(IEnumerable<Node> Nodes) : ICommandRequest<Result>
{
    public IEnumerable<Change> GetChanges()
    {
        return Nodes.Select(n => Change.For<DeleteNodes>(n.NodeId, ChangeType.Deleted, $"Deleted {n.Type} {n.Name}"));
    }
}

[UsedImplicitly]
internal class DeleteNodesHandler(IConnectionManager manager) : IRequestHandler<DeleteNodes, Result>
{
    private const string DeleteNode = "DELETE FROM Node WHERE NodeId = @NodeId";

    public async Task<Result> Handle(DeleteNodes request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        await connection.ExecuteAsync(DeleteNode, request.Nodes);
        await manager.Vacuum(connection);
        return Result.Ok();
    }
}