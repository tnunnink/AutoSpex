using AutoSpex.Engine;
using Dapper;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListReferences(string Key) : IRequest<IEnumerable<Reference>>;

[UsedImplicitly]
internal class ListReferencesHandler(IConnectionManager manager)
    : IRequestHandler<ListReferences, IEnumerable<Reference>>
{
    private const string ListSourceNames = "SELECT Name FROM Source WHERE Name LIKE '%' || @Key || '%'";
    private const string ListReferences = "SELECT Scope FROM Reference WHERE Scope LIKE '%' || @Key || '%'";

    public async Task<IEnumerable<Reference>> Handle(ListReferences request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        //If no path separator exists, just return the source names.
        //This is what the path must start with if it is not a relative path.
        if (!request.Key.Contains('/'))
        {
            var names = await connection.QueryAsync<string>(ListSourceNames, new { request.Key });
            return names.Select(n => new Reference(n));
        }

        var scopes = await connection.QueryAsync<string>(ListReferences, new { request.Key });
        return scopes.Select(s => new Reference(s));
    }
}