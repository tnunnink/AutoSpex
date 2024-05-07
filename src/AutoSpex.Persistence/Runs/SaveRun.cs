using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SaveRun(Run Run) : IDbCommand<Result>;

[UsedImplicitly]
internal class SaveRunHandler(IConnectionManager manager) : IRequestHandler<SaveRun, Result>
{
    private const string UpsertRun =
        """
        INSERT INTO Run (RunId, NodeId, SourceId, Name, Result, RanOn, RanBy)
        VALUES (@RunId, @NodeId, @SourceId, @Name, @Result, @RanOn, @RanBy)
        ON CONFLICT DO UPDATE
            SET NodeId = @NodeId, SourceId = @SourceId, Name = @Name, Result = @Result, RanOn = @RanOn, RanBy = @RanBy;
        """;

    private const string DeleteOutcomes = "DELETE FROM Outcome WHERE RunId = @RunId";

    private const string InsertOutcomes =
        """
        INSERT INTO Outcome (OutcomeId, RunId, SpecId, Result, Duration, Total, Passed, Failed, Errored, Evaluations)
        VALUES (@OutcomeId, @RunId, @SpecId, @Result, @Duration, @Total, @Passed, @Failed, @Errored, @Evaluations)
        """;

    public async Task<Result> Handle(SaveRun request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(UpsertRun, new { request.Run }, transaction);
        await connection.ExecuteAsync(DeleteOutcomes, new { request.Run.RunId }, transaction);
        await connection.ExecuteAsync(InsertOutcomes, new { request.Run.Outcomes }, transaction);

        transaction.Commit();
        return Result.Ok();
    }
}