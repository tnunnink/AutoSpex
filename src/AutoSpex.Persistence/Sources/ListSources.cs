using AutoSpex.Engine;
using Dapper;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListSources : IRequest<IEnumerable<Source>>;

[UsedImplicitly]
internal class ListSourcesHandler(IConnectionManager manager) : IRequestHandler<ListSources, IEnumerable<Source>>
{
    private const string ListSources =
        """
        SELECT SourceId, Name, IsTarget, TargetName, TargetType, ExportedOn, ExportedBy, Description 
        FROM Source ORDER BY Name
        """;

    public async Task<IEnumerable<Source>> Handle(ListSources request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var results = await connection.QueryAsync<Source>(ListSources);
        return results;
    }
}