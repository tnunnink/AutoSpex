using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SaveSpec(Spec Spec) : IDbCommand<Result>, IDbLoggable
{
    public Guid NodeId => Spec.SpecId;
    public string Message => $"Saved Spec '{Spec.Name}'";
}

[UsedImplicitly]
internal class SaveSpecHandler(IConnectionManager manager) : IRequestHandler<SaveSpec, Result>
{
    private const string NodeExists = "SELECT COUNT() FROM Node WHERE NodeId = @NodeId";

    private const string UpsertSpec =
        "INSERT INTO Spec(SpecId, Element, Specification) VALUES (@SpecId, @Element, @Specification) " +
        "ON CONFLICT DO UPDATE SET Element = @Element, Specification = @Specification;";

    public async Task<Result> Handle(SaveSpec request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var exists = await connection.QuerySingleAsync<int>(NodeExists, new { NodeId = request.Spec.SpecId });
        if (exists == 0) return Result.Fail($"Node not found: {request.Spec.SpecId}");

        var record = new
        {
            request.Spec.SpecId,
            request.Spec.Query.Element,
            Specification = request.Spec.Serialize()
        };

        await connection.ExecuteAsync(UpsertSpec, record);

        return Result.Ok();
    }
}