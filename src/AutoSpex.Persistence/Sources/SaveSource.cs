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

    private const string DeleteOverrides =
        "DELETE FROM Override WHERE SourceId = @SourceId";

    private const string InsertOverride =
        """
        INSERT INTO Override (OverrideId, SourceId, VariableId, Value) 
        VALUES (@OverrideId, @SourceId, @VariableId, @Value)
        """;

    public async Task<Result> Handle(SaveSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var exists = await connection.QuerySingleAsync<int>(Exists, new { request.Source.SourceId });
        if (exists == 0)
            return Result.Fail($"Source not found: {request.Source.SourceId}");

        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(DeleteOverrides, new { request.Source.SourceId }, transaction);

        var overrides = request.Source.Overrides.Select(x => new
            {
                OverrideId = Guid.NewGuid(),
                request.Source.SourceId,
                x.VariableId,
                x.Value
            }
        );

        await connection.ExecuteAsync(InsertOverride, overrides, transaction);

        transaction.Commit();

        return Result.Ok();
    }
}