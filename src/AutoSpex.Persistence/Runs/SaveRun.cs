using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SaveRun(Run Run) : IDbCommand<Result>, IDbLoggable
{
    public Guid NodeId => Run.RunId;
    public string Message => $"Saved Run '{Run.Name}'";
}

[UsedImplicitly]
internal class SaveRunHandler(IConnectionManager manager) : IRequestHandler<SaveRun, Result>
{
    private const string RunExists =
        "SELECT COUNT() FROM Run WHERE RunId = @RunId";

    private const string UpdateRun =
        "UPDATE RUN SET Result = @Result, RanOn = @RanOn, RanBy = @RanBy WHERE RunId = @RunId";

    private const string UpsertOutcomes =
        """
        INSERT INTO Outcome (OutcomeId, RunId, SpecId, SourceId, Result, Duration, Evaluations)
        VALUES (@OutcomeId, @RunId, @SpecId, @SourceId, @Result, @Duration, @Evaluations)
        ON CONFLICT DO UPDATE SET SpecId = @SpecId, SourceId = @SourceId,
                                  Result = @Result, Duration = @Duration, Evaluations = @Evaluations
        """;

    public async Task<Result> Handle(SaveRun request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        using var transaction = connection.BeginTransaction();

        var exists = connection.QuerySingleOrDefault<int>(RunExists, new { request.Run.RunId });
        if (exists != 1) return Result.Fail($"Run not found: {request.Run.RunId}");

        await connection.ExecuteAsync(UpdateRun, request.Run, transaction);

        await connection.ExecuteAsync(UpsertOutcomes,
            request.Run.Outcomes.Select(outcome => new
            {
                outcome.OutcomeId,
                request.Run.RunId,
                SpecId = outcome.Spec?.NodeId,
                SourceId = outcome.Source?.NodeId,
                outcome.Result,
                outcome.Duration,
                Evaluations = outcome.GetEvaluationData()
            }), transaction);

        //3. upsert the Override table

        transaction.Commit();
        return Result.Ok();
    }
}