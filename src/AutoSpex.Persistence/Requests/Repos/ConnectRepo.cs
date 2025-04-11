using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ConnectRepo(Repo Repo) : IRequest<Result>;

[UsedImplicitly]
internal class ConnectRepoHandler(IConnectionManager manager) : IRequestHandler<ConnectRepo, Result>
{
    private const string ConnectRepo =
        """
        INSERT INTO Repo (RepoId, Location) 
        VALUES (@RepoId, @Location)
        ON CONFLICT (Location) DO UPDATE SET LastConnected = @LastConnected 
        """;

    public async Task<Result> Handle(ConnectRepo request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var record = new
        {
            request.Repo.RepoId,
            request.Repo.Location,
            LastConnected = DateTime.UtcNow
        };

        await connection.ExecuteAsync(ConnectRepo, record);

        return Result.Ok();
    }
}