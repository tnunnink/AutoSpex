using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Features.Criteria;
using AutoSpex.Client.Services;
using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Specifications;

[PublicAPI]
public record LoadSpecRequest(SpecificationViewModel Spec) : IRequest<Result>;

public class LoadSpecHandler : IRequestHandler<LoadSpecRequest, Result>
{
    private readonly ProjectDatabase _database;

    private const string GetSpec = "SELECT * FROM Spec WHERE SpecId = @SpecId";

    private const string GetCriterion = "SELECT * FROM Criterion WHERE NodeId = @SpecId";

    public LoadSpecHandler(ProjectDatabase database)
    {
        _database = database;
    }

    public Task<Result> Handle(LoadSpecRequest request, CancellationToken cancellationToken)
    {
        return Result.Try(() => Execute(request, cancellationToken),
            e => new Error($"Failed to load specification '{request.Spec.Node.Name}' with error '{e.Message}'")
                .CausedBy(e));
    }

    private async Task Execute(LoadSpecRequest request, CancellationToken cancellationToken)
    {
        using var connection = await _database.Connect(cancellationToken);

        var details = request.Spec;

        var spec = await connection.QuerySingleOrDefaultAsync(GetSpec, new {SpecId = details.Node.NodeId});

        if (spec is null) return;

        request.Spec.Element = Element.FromName(spec.Element);

        var criteria = (await connection.QueryAsync(GetCriterion, new {SpecId = details.Node.NodeId}))
            .Select(r => new CriterionViewModel(r))
            .ToList();

        foreach (var criterion in criteria.Where(c => c.Usage == CriterionUsage.Filter))
        {
            details.Filters.Add(criterion);
        }

        foreach (var criterion in criteria.Where(c => c.Usage == CriterionUsage.Verification))
        {
            details.Verifications.Add(criterion);
        }
    }
}