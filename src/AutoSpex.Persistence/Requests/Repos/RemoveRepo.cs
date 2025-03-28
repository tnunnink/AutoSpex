using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record RemoveRepo(Guid RepoId) : IRequest<Result>;

[UsedImplicitly]
internal class RemoveRepoHandler(IConnectionManager manager) : IRequestHandler<RemoveRepo, Result>
{
    private const string RemoveRepo = "DELETE FROM Repo WHERE RepoId = @RepoId";

    public async Task<Result> Handle(RemoveRepo request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        await connection.ExecuteAsync(RemoveRepo, new { request.RepoId });
        return Result.Ok();
    }
}