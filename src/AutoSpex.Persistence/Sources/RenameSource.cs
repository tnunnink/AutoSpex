﻿using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record RenameSource(Source Source) : IDbCommand<Result>;

[UsedImplicitly]
internal class RenameSourceHandler(IConnectionManager manager) : IRequestHandler<RenameSource, Result>
{
    private const string Rename = "UPDATE Source SET Name = @Name WHERE SourceId = @SourceId";

    public async Task<Result> Handle(RenameSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var updated = await connection.ExecuteAsync(Rename, new { request.Source.SourceId, request.Source.Name });
        return Result.OkIf(updated == 1, $"Source not found: '{request.Source.SourceId}'");
    }
}