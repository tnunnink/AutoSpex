using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record RemoveRepos(IEnumerable<Repo> Repos) : IRequest<Result>;

[UsedImplicitly]
internal class RemoveRepoHandler(IConnectionManager manager) : IRequestHandler<RemoveRepos, Result>
{
    private const string RemoveRepo = "DELETE FROM Repo WHERE RepoId = @RepoId";

    public async Task<Result> Handle(RemoveRepos request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        await connection.ExecuteAsync(RemoveRepo, request.Repos);
        await connection.Vacuum();
        return Result.Ok();
    }
}