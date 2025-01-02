using AutoSpex.Engine;
using Dapper;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListRunsFor(Guid Id) : IRequest<IEnumerable<Run>>;

[UsedImplicitly]
internal class ListRunsForHandler(IConnectionManager manager) : IRequestHandler<ListRunsFor, IEnumerable<Run>>
{
    private const string ListRuns =
        """
        SELECT RunId, Name, Node, Source, Result, RanOn, RanBy, Duration, PassRate 
        FROM RUN
        WHERE Node LIKE '%' || @Id || '%' or Source LIKE '%' || @Id || '%'
        ORDER BY RanOn DESC
        """;
    
    public async Task<IEnumerable<Run>> Handle(ListRunsFor request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var runs = await connection.QueryAsync<Run>(ListRuns, new { Id = request.Id.ToString() });
        return runs;
    }
}