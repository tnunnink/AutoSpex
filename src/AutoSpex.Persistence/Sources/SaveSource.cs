﻿using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SaveSource(Source Source) : IDbCommand<Result>;

[UsedImplicitly]
internal class SaveSourceHandler(IConnectionManager manager) : IRequestHandler<SaveSource, Result>
{
    private const string Exists =
        "SELECT COUNT() FROM Source WHERE SourceId = @SourceId";

    private const string DeleteSuppressions =
        "DELETE FROM Suppression WHERE SourceId = @SourceId";

    private const string DeleteOverrides =
        "DELETE FROM Override WHERE SourceId = @SourceId";

    private const string InsertSuppressions =
        "INSERT INTO Suppression (SourceId, NodeId, Reason) VALUES (@SourceId, @NodeId, @Reason)";

    private const string InsertOverride =
        "INSERT INTO Override (SourceId, NodeId, Config) VALUES (@SourceId, @NodeId, @Config)";

    public async Task<Result> Handle(SaveSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var exists = await connection.QuerySingleAsync<int>(Exists, new { request.Source.SourceId });
        if (exists == 0)
            return Result.Fail($"Source not found: {request.Source.SourceId}");

        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(DeleteSuppressions, new { request.Source.SourceId }, transaction);
        await connection.ExecuteAsync(DeleteOverrides, new { request.Source.SourceId }, transaction);

        await connection.ExecuteAsync(InsertSuppressions,
            request.Source.Suppressions.Select(x => new
            {
                request.Source.SourceId,
                x.NodeId,
                x.Reason
            }),
            transaction);

        await connection.ExecuteAsync(InsertOverride,
            request.Source.Overrides.Select(x => new
                {
                    request.Source.SourceId,
                    x.NodeId,
                    Config = x.Spec
                }
            ),
            transaction);

        transaction.Commit();
        return Result.Ok();
    }
}