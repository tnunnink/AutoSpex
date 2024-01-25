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
    private const string Query = "SELECT SourceId, Name, TargetType, TargetName, ExportedOn FROM Source";

    public async Task<Result<IEnumerable<Source>>> Handle(ListSources request, CancellationToken cancellationToken)
    {
        var connection = await manager.Connect(Database.Project, cancellationToken);
        var sources = await connection.QueryAsync<Source>(Query);
        return Result.Ok(sources);
    }
}