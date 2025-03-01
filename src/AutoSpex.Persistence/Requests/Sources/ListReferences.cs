using AutoSpex.Engine;
using Dapper;
using JetBrains.Annotations;
using L5Sharp.Core;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListReferences(string Key) : IRequest<IEnumerable<Reference>>;

[UsedImplicitly]
internal class ListReferencesHandler(IConnectionManager manager)
    : IRequestHandler<ListReferences, IEnumerable<Reference>>
{
    private const string ListSourceNames = "SELECT Name FROM Source WHERE Name LIKE '%' || @Key || '%'";
    private const string GetContentByTarget = "SELECT Content FROM Source WHERE IsTarget = 1";
    private const string GetContentByName = "SELECT Content FROM Source WHERE Name = @Name";

    public async Task<IEnumerable<Reference>> Handle(ListReferences request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        //What we return will depend on if and where the first path separator is in the request text.
        var index = request.Key.IndexOf('/');

        //If no path separator exists, just return the source names.
        //This is what the path must start with if it is not a relative path.
        if (index == -1)
        {
            var names = await connection.QueryAsync<string>(ListSourceNames, new { request.Key });
            return names.Select(n => new Reference(n));
        }

        //The first part up to path separator is the source name. We either have a specific source we can get
        //or if  a relative path then we default to the target. If neither produce a result we return empty collection.
        var sourceName = request.Key[..index];
        var content = string.IsNullOrEmpty(sourceName)
            ? await connection.QuerySingleOrDefaultAsync<L5X>(GetContentByTarget)
            : await connection.QuerySingleOrDefaultAsync<L5X>(GetContentByName, new { Name = sourceName });

        //We want to repalce the controller name with the source name in this case and return these as references.
        var scopes = content?.Scopes().Select(s => Scope.To($"{sourceName}/{s.LocalPath}")) ?? [];
        return scopes.Where(s => s.Path.Contains(request.Key)).Select(s => new Reference(s));
    }
}