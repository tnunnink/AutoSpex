using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record UpdateRepo(Repo Repo) : IRequest<Result>;

[UsedImplicitly]
internal class UpdateRepoHandler(IConnectionManager manager) : IRequestHandler<UpdateRepo, Result>
{
    private const string UpdateLocation = "UPDATE Repo SET Location = @Location WHERE RepoId = @RepoId";

    public async Task<Result> Handle(UpdateRepo request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var result = await connection.ExecuteAsync(UpdateLocation, request.Repo);
        return Result.OkIf(result == 1, $"Repository not found: '{request.Repo.Location}'");
    }
}