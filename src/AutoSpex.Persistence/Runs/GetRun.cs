using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetRun(Guid RunId) : IDbCommand<Result<Run>>;

[UsedImplicitly]
internal class GetRunHandler(IConnectionManager manager) : IRequestHandler<GetRun, Result<Run>>
{
    private const string GetRun =
        """
        SELECT r.RunId, n.Name, r.Result, r.RanOn, r.RanBy
            FROM Node n
            JOIN Run r on n.NodeId = r.RunId
            WHERE NodeId = @RunId
        """;

    private const string GetOutcomes =
        """
        SELECT OutcomeId, Result, Duration, Evaluations, SpecId, spec.Name, spec.Type, SourceId, spec.ParentId, spec.Name, spec.Type
        FROM Outcome o
        LEFT JOIN Node spec on spec.NodeId = o.SpecId
        LEFT JOIN Node source on source.NodeId = o.SourceId
        WHERE RunId = @RunId
        """;

    public async Task<Result<Run>> Handle(GetRun request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        var run = await connection.QuerySingleOrDefaultAsync<Run>(GetRun, new { request.RunId });
        if (run is null) return Result.Fail($"Run not found: '{request.RunId}'");

        var outcomes = await connection.QueryAsync<Outcome, Node, Node, Outcome>(GetOutcomes,
            (outcome, spec, source) => outcome.ConfigureSpec(spec).ConfigureSource(source),
            splitOn: "SpecId,SourceId",
            param: new { request.RunId });

        run.AddOutcomes(outcomes);

        //todo overrides?

        return Result.Ok(run);
    }
}