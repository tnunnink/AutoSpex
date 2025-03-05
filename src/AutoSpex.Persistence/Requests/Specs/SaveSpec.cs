using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SaveSpec(Node Node) : IRequest<Result>;

[UsedImplicitly]
internal class SaveSpecHandler(IConnectionManager manager) : IRequestHandler<SaveSpec, Result>
{
    private const string Exists = "SELECT COUNT() FROM Spec WHERE SpecId = @SpecId";
    private const string Update = "UPDATE Spec SET Config = @Config WHERE SpecId = @SpecId";

    public async Task<Result> Handle(SaveSpec request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var spec = request.Node.Spec;

        var exists = await connection.QuerySingleAsync<int>(Exists, new { spec.SpecId });
        if (exists != 1) return Result.Fail($"Spec not found: {spec.SpecId}");

        await connection.ExecuteAsync(Update, new { spec.SpecId, Config = spec });
        return Result.Ok();
    }
}