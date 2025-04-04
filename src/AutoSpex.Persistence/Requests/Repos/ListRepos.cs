using AutoSpex.Engine;
using Dapper;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListRepos : IRequest<IEnumerable<Repo>>;

[UsedImplicitly]
internal class ListReposHandler(IConnectionManager manager) : IRequestHandler<ListRepos, IEnumerable<Repo>>
{
    private const string ListRepos = "SELECT RepoId, Location FROM Repo ORDER BY LastConnected DESC";

    public async Task<IEnumerable<Repo>> Handle(ListRepos request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var repos = await connection.QueryAsync<Repo>(ListRepos);
        return repos;
    }
}