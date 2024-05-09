using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListSources : IDbQuery<Result<IEnumerable<Source>>>;

[UsedImplicitly]
internal class ListSourcesHandler(IConnectionManager manager)
    : IRequestHandler<ListSources, Result<IEnumerable<Source>>>
{
    private const string ListSources =
        """
        SELECT SourceId, Name, IsSelected, TargetType, TargetName, ExportedOn, ExportedBy
        FROM Source
        """;

    public async Task<Result<IEnumerable<Source>>> Handle(ListSources request, CancellationToken cancellationToken)
    {
        var connection = await manager.Connect(Database.Project, cancellationToken);
        var sources = await connection.QueryAsync<Source>(ListSources);
        return Result.Ok(sources);
    }
}