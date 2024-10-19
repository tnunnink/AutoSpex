using AutoSpex.Engine;
using Dapper;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListRuns : IRequest<IEnumerable<Run>>;

[UsedImplicitly]
internal class ListRunsHandler(IConnectionManager manager) : IRequestHandler<ListRuns, IEnumerable<Run>>
{
    private const string ListRuns = "SELECT RunId, Name, Node, Source, Result, RanOn, RanBy FROM RUN";

    public async Task<IEnumerable<Run>> Handle(ListRuns request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var runs = await connection.QueryAsync<Run>(ListRuns);
        return runs;
    }
}