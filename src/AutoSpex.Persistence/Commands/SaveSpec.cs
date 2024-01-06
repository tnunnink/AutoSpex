using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SaveSpecRequest(Guid NodeId, Specification Specification) : ICommand<Result>;

[UsedImplicitly]
internal class SaveSpecHandler(IConnectionManager manager) : IRequestHandler<SaveSpecRequest, Result>
{
    private const string UpsertSpec = "INSERT INTO Spec(SpecId, Element) VALUES (@SpecId, @Element) " +
                                      "ON CONFLICT DO UPDATE SET Element = @Element";

    private const string UpsertCriterion =
        "INSERT INTO Criterion(CriterionId, NodeId, Usage, Element, Property, Operation) " +
        "VALUES (@CriterionId, @NodeId, @Usage, @Element, @Property, @Operation)" +
        "ON CONFLICT DO UPDATE SET Element = @Element, Property = @Property, Operation = @Operation";

    private const string DeleteOrphaned =
        "DELETE FROM Criterion WHERE NodeId = @NodeId AND CriterionId NOT IN @CriterionIds";

    public async Task<Result> Handle(SaveSpecRequest request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        using var transaction = connection.BeginTransaction();

        /*//1. Update the spec table with the correct element.
        var spec = new SpecRecord(request.NodeId, request.Specification.Element);
        await connection.ExecuteAsync(UpsertSpec, spec, transaction);
        
        var options = new SpecOptionsRecord(
            request.NodeId,
            request.Specification.Options.RunToEnd,
            request.Specification.Options.FilterInclusion);*/

        transaction.Commit();

        return Result.Ok();
    }
}