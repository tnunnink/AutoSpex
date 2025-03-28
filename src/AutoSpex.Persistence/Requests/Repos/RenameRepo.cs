using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record RenameRepo(Repo Repo) : IRequest<Result>;

[UsedImplicitly]
internal class RenameRepoHandler(IConnectionManager provider) : IRequestHandler<RenameRepo, Result>
{
    private const string Rename = "UPDATE Repo SET Name = @Name WHERE RepoId = @RepoId";

    public async Task<Result> Handle(RenameRepo request, CancellationToken cancellationToken)
    {
        using var connection = await provider.Connect(cancellationToken);
        var result = await connection.ExecuteAsync(Rename, request.Repo);
        return Result.OkIf(result == 1, $"Repository not found: {request.Repo.RepoId}");
    }
}