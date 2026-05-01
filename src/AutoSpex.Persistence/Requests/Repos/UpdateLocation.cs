using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record UpdateLocation(Guid RepoId, string Location) : IRequest<Result<Repo>>;

[UsedImplicitly]
internal class UpdateLocationHandler(IConnectionManager manager) : IRequestHandler<UpdateLocation, Result<Repo>>
{
    private const string RepoExists = "SELECT COUNT() FROM Repo WHERE RepoId = @RepoId";
    private const string UpdateLocation = "UPDATE Repo SET Location = @Location WHERE RepoId = @RepoId";
    private const string DeleteTargets = "DELETE FROM Target WHERE RepoId = @RepoId";

    public async Task<Result<Repo>> Handle(UpdateLocation request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        if (await connection.QuerySingleAsync<int>(RepoExists, new { request.RepoId }) != 1)
        {
            return Result.Fail($"Repo not found: {request.RepoId}");
        }

        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(DeleteTargets, new { request.RepoId }, transaction);
        await connection.ExecuteAsync(UpdateLocation, new { request.RepoId, request.Location }, transaction);

        transaction.Commit();
        return Result.Ok(Repo.Configure(request.Location));
    }
}