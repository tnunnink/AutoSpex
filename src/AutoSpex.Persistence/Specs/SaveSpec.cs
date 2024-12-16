using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SaveSpec(Spec Spec) : IDbCommand<Result>;

[UsedImplicitly]
internal class SaveSpecHandler(IConnectionManager manager) : IRequestHandler<SaveSpec, Result>
{
    private const string Exists = "SELECT COUNT() FROM Spec WHERE SpecId = @SpecId";
    private const string Update = "UPDATE Spec SET Config = @Config WHERE SpecId = @SpecId";

    public async Task<Result> Handle(SaveSpec request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        using var transaction = connection.BeginTransaction();

        var exists = await connection.QuerySingleAsync<int>(Exists, new { request.Spec.SpecId });
        if (exists != 1) return Result.Fail($"Spec not found: {request.Spec.SpecId}");

        await connection.ExecuteAsync(Update, new { request.Spec.SpecId, Config = request.Spec }, transaction);
        transaction.Commit();
        return Result.Ok();
    }
}