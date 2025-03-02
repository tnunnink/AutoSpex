using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DeleteRuns(IEnumerable<Run> Runs) : ICommandRequest<Result>
{
    public IEnumerable<Change> GetChanges()
    {
        return Runs.Select(x => Change.For<DeleteRuns>(x.RunId, ChangeType.Deleted, $"Deleted Run for {x.Name}"));
    }
}

[UsedImplicitly]
internal class DeleteRunsHandler(IConnectionManager manager) : IRequestHandler<DeleteRuns, Result>
{
    private const string DeleteRun = "DELETE FROM Run WHERE RunId = @RunId";

    public async Task<Result> Handle(DeleteRuns request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var result = await connection.ExecuteAsync(DeleteRun, request.Runs);
        await connection.Vacuum();
        return Result.Ok().WithSuccess($"Successfully deleted {result} runs");
    }
}