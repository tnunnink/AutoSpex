using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record UpdateTargets(Repo Repo) : IRequest<Result>;

internal class UpdateTargetsHandler(IConnectionManager manager) : IRequestHandler<UpdateTargets, Result>
{
    private const string RepoExists = "SELECT COUNT() FROM Repo WHERE RepoId = @RepoId";
    private const string DeleteTargets = "DELETE FROM Target WHERE RepoId = @RepoId";

    private const string InsertTarget =
        """
        INSERT INTO Target (TargetId, RepoId, Location) 
        VALUES (@TargetId, @RepoId, @Location)
        """;

    public async Task<Result> Handle(UpdateTargets request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        if (await connection.QuerySingleAsync<int>(RepoExists, new { request.Repo.RepoId }) != 1)
        {
            return Result.Fail($"Repo not found: {request.Repo.RepoId}");
        }

        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(DeleteTargets, new { request.Repo.RepoId }, transaction);

        await connection.ExecuteAsync(InsertTarget,
            request.Repo.Targets.Select(t => new
            {
                TargetId = Guid.NewGuid().ToString(),
                request.Repo.RepoId,
                t.Location
            }),
            transaction);

        transaction.Commit();
        return Result.Ok();
    }
}