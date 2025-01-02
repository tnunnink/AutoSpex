using Dapper;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListSourceNames : IRequest<IEnumerable<string>>;

[UsedImplicitly]
internal class ListSourceNamesHandler(IConnectionManager manager)
    : IRequestHandler<ListSourceNames, IEnumerable<string>>
{
    private const string ListSources = "SELECT Name FROM Source";

    public async Task<IEnumerable<string>> Handle(ListSourceNames request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var names = await connection.QueryAsync<string>(ListSources);
        return names;
    }
}