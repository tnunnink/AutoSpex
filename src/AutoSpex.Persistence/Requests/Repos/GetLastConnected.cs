using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetLastConnected : IRequest<Result<Repo>>;

[UsedImplicitly]
internal class GetLastConnectedHandler(IConnectionManager manager) : IRequestHandler<GetLastConnected, Result<Repo>>
{
    private const string GetLastConnected = "SELECT RepoId, Location FROM Repo ORDER BY LastConnected DESC LIMIT 1";

    public async Task<Result<Repo>> Handle(GetLastConnected request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var repo = await connection.QueryFirstOrDefaultAsync<Repo>(GetLastConnected);
        return repo is not null ? Result.Ok(repo) : Result.Fail("No repos exist in the database.");
    }
}