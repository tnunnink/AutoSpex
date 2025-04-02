using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SaveRepo(Repo Repo) : IRequest<Result>;

[UsedImplicitly]
internal class SaveRepoHandler(IConnectionManager manager) : IRequestHandler<SaveRepo, Result>
{
    private const string UpdateLocation =
        "UPDATE Repo SET Location = @Location, Name = @Name WHERE RepoId = @RepoId";

    public async Task<Result> Handle(SaveRepo request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var result = await connection.ExecuteAsync(UpdateLocation, request.Repo);
        return Result.OkIf(result == 1, $"Repository not found: '{request.Repo.Location}'");
    }
}