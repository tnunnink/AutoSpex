using AutoSpex.Engine;
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

    private const string DeleteAction =
        "DELETE FROM Action WHERE SourceId = @SourceId";

    private const string InsertAction =
        """
        INSERT INTO Action (SourceId, NodeId, Type, Reason, Config) 
        VALUES (@SourceId, @NodeId, @Type, @Reason, @Config)
        """;

    public async Task<Result> Handle(SaveSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var exists = await connection.QuerySingleAsync<int>(Exists, new { request.Source.SourceId });
        if (exists == 0)
            return Result.Fail($"Source not found: {request.Source.SourceId}");

        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(DeleteAction, new { request.Source.SourceId }, transaction);

        await connection.ExecuteAsync(InsertAction,
            request.Source.Rules.Select(x => new
            {
                request.Source.SourceId,
                x.NodeId,
                Type = x.Type,
                x.Reason,
                x.Config
            }),
            transaction);

        transaction.Commit();
        return Result.Ok();
    }
}