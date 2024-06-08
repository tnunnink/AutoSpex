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

    public string Message =>
        $"Saved {Spec.Element} Spec '{Spec.Name}' with {Spec.Filters.Count} filters and {Spec.Verifications.Count} verifications";
}

[UsedImplicitly]
internal class SaveSpecHandler(IConnectionManager manager) : IRequestHandler<SaveSpec, Result>
{
    private const string HasNode = "SELECT COUNT(NodeId) FROM Node WHERE NodeId = @NodeId";

    private const string UpsertSpec =
        "INSERT INTO Spec(SpecId, Element, Specification) VALUES (@SpecId, @Element, @Specification) " +
        "ON CONFLICT DO UPDATE SET Element = @Element, Specification = @Specification;";

    public async Task<Result> Handle(SaveSpec request, CancellationToken cancellationToken)
    {
        if (request.Spec.SpecId == Guid.Empty)
            return Result.Fail("Can not save spec with empty id.");

        using var connection = await manager.Connect(Database.Project, cancellationToken);

        //First check that the node exists, so we don't get a SQL exception.
        var exists = await connection.QuerySingleAsync<int>(HasNode, new { NodeId = request.Spec.SpecId });
        if (exists == 0) return Result.Fail($"Node not found: {request.Spec.SpecId}");

        //If so serialize the spec and upsert the data.
        var record = new { request.Spec.SpecId, request.Spec.Element, Specification = request.Spec.Serialize() };
        await connection.ExecuteAsync(UpsertSpec, record);

        return Result.Ok();
    }
}