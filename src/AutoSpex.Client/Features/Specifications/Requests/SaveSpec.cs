using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Behaviors;
using AutoSpex.Client.Services;
using Avalonia.Controls.Notifications;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Specifications;

[PublicAPI]
public record SaveSpecRequest(SpecificationViewModel Specification) : INotifiableRequest<Result>
{
    public Notification BuildNotification(Result result)
    {
        if (result.IsFailed)
        {
            return new Notification("Save Failed",
                $"{result.Errors.First().Message}. Please see notification for details.",
                NotificationType.Error);
        }

        return new Notification("Save Complete",
            $"Specification {Specification.Node.Name} saved successfully at {DateTime.Now}",
            NotificationType.Success);
    }
}

[UsedImplicitly]
public class SaveSpecHandler : IRequestHandler<SaveSpecRequest, Result>
{
    private readonly ProjectDatabase _database;

    private const string UpsertSpec = "INSERT INTO Spec(SpecId, Element) VALUES (@SpecId, @Element) " +
                                      "ON CONFLICT DO UPDATE SET Element = @Element";

    private const string UpsertCriterion =
        "INSERT INTO Criterion(CriterionId, NodeId, Usage, Element, Property, Operation, Args) " +
        "VALUES (@CriterionId, @NodeId, @Usage, @Element, @Property, @Operation, @Args)" +
        "ON CONFLICT DO UPDATE SET Element = @Element, Property = @Property, Operation = @Operation, Args = @Args";

    private const string DeleteOrphaned =
        "DELETE FROM Criterion WHERE NodeId = @NodeId AND CriterionId NOT IN @CriterionIds";

    public SaveSpecHandler(ProjectDatabase database)
    {
        _database = database;
    }

    public Task<Result> Handle(SaveSpecRequest request, CancellationToken cancellationToken)
    {
        return Result.Try(() => Execute(request, cancellationToken),
            e => new Error($"Specification '{request.Specification.Node.Name}' failed to save with error {e.Message}.")
                .CausedBy(e));
    }

    private async Task Execute(SaveSpecRequest request, CancellationToken cancellationToken)
    {
        using var connection = await _database.Connect(cancellationToken);
        var transaction = connection.BeginTransaction();

        var details = request.Specification;

        //1. Update the spec table with the correct element.
        var spec = new {SpecId = details.Node.NodeId, Element = details.Element?.Name};
        await connection.ExecuteAsync(UpsertSpec, spec, transaction);

        //2. Save all filters
        foreach (var filter in details.Filters)
        {
            await connection.ExecuteAsync(UpsertCriterion, filter.ToRecord(), transaction);
        }

        //3. Save all verification
        foreach (var verification in details.Verifications)
        {
            await connection.ExecuteAsync(UpsertCriterion, verification.ToRecord(), transaction);
        }

        //5. Delete orphaned criterion
        var nodeId = details.Node.NodeId.ToString();
        var criterionIds = details.Filters.Select(f => f.CriterionId.ToString())
            .Concat(details.Verifications.Select(v => v.CriterionId.ToString()))
            .Distinct();
        await connection.ExecuteAsync(DeleteOrphaned, new {NodeId = nodeId, CriterionIds = criterionIds}, transaction);

        transaction.Commit();
    }
}