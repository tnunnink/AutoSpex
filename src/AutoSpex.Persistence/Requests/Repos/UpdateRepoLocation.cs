using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record UpdateRepoLocation(Guid RepoId, string Location) : IRequest<Result<Repo>>;

[UsedImplicitly]
internal class UpdateRepoLocationHandler(IConnectionManager manager) : IRequestHandler<UpdateRepoLocation, Result<Repo>>
{
    private const string UpdateLocation = "UPDATE Repo SET Location = @Location WHERE RepoId = @RepoId";

    public async Task<Result<Repo>> Handle(UpdateRepoLocation request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var result = await connection.ExecuteAsync(UpdateLocation, new { request.RepoId, request.Location });

        return result == 1
            ? Result.Ok(Repo.Configure(request.Location))
            : Result.Fail($"Repository not found: '{request.RepoId}'");
    }
}