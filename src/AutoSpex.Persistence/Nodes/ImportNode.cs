﻿using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ImportNode(Package Package, ImportAction Action) : ICommandRequest<Result<Node>>
{
    public IEnumerable<Change> GetChanges()
    {
        return Package.Collection.DescendantsAndSelf().Select(n =>
            Change.For<ImportNode>(n.NodeId, ChangeType.Created, $"Imported {n.Type} {n.Name}"));
    }
}

[UsedImplicitly]
internal class ImportNodeHandler(IConnectionManager manager) : IRequestHandler<ImportNode, Result<Node>>
{
    private const string DeleteNode =
        "DELETE FROM Node WHERE Type = 'Collection'AND Name = @Name";

    private const string InsertNode =
        """
        INSERT INTO Node (NodeId, ParentId, Type, Name, Description)
        VALUES (@NodeId, @ParentId, @Type, @Name, @Description)
        """;

    private const string InsertSpec =
        """
        INSERT INTO Spec ([SpecId], [NodeId], [Config])
        VALUES (@SpecId, @NodeId, @Config)
        """;

    public async Task<Result<Node>> Handle(ImportNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        using var transaction = connection.BeginTransaction();

        //todo how would we handle version changes here.
        //todo we would need to migrate the data as needed.

        var collection = request.Package.Collection;

        if (request.Action == ImportAction.Repalce)
        {
            await connection.ExecuteAsync(DeleteNode, new { collection.Name }, transaction);
        }

        var import = request.Action == ImportAction.Copy
            ? collection.Duplicate($"{collection.Name} Copy")
            : collection;

        foreach (var node in import.DescendantsAndSelf())
        {
            await connection.ExecuteAsync(InsertNode, node, transaction);

            await connection.ExecuteAsync(InsertSpec,
                new { node.Spec.SpecId, node.NodeId, Config = node.Spec },
                transaction);
        }

        transaction.Commit();

        return Result.Ok(import);
    }
}