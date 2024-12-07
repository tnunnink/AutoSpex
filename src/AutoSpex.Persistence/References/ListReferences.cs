using AutoSpex.Engine;
using Dapper;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence.References;

[PublicAPI]
public record ListReferences(string Key) : IRequest<IEnumerable<Reference>>;

[UsedImplicitly]
internal class ListReferencesHandler(IConnectionManager manager)
    : IRequestHandler<ListReferences, IEnumerable<Reference>>
{
    private const string GetSourceId =
        "SELECT SourceId FROM Source WHERE Name = @Name";

    private const string ListSourceNames =
        "SELECT Name FROM Source WHERE Name LIKE '%' || @Key || '%'";

    private const string ListScopesWith =
        "SELECT Scope FROM Reference WHERE Scope LIKE '%' || @Key || '%'";

    private const string ListScopesFor =
        """
        SELECT S.Name || Scope 
        FROM Reference R 
        JOIN Source S on R.SourceId = S.SourceId
        WHERE R.SourceId = @SourceId AND Scope LIKE '%' || @Path || '%'
        """;

    public async Task<IEnumerable<Reference>> Handle(ListReferences request, CancellationToken cancellationToken)
    {
        var connection = await manager.Connect(cancellationToken);

        //What we return will depend on if and where the first path separator is in the request text.
        var index = request.Key.IndexOf('/');

        switch (index)
        {
            //If no path separator exists, just return the source names.
            //This is what the path must start with if it is not a relative path.
            case -1:
            {
                var names = await connection.QueryAsync<string>(ListSourceNames, new { request.Key });
                return names.Select(n => new Reference(n));
            }
            //This is a relative path, so we will ignore any specific source
            //and return all matching references.
            case 0:
            {
                var relative = await connection.QueryAsync<string>(ListScopesWith, new { request.Key });
                return relative.Select(r => new Reference(r));
            }
        }

        //Otherwise, we get the source and corresponding references that match the path of the input text.
        var source = request.Key[..index];
        var path = request.Key[index..];

        var id = await connection.QuerySingleOrDefaultAsync<Guid>(GetSourceId, new { Name = source });
        if (id == Guid.Empty) return [];

        var scopes = await connection.QueryAsync<string>(ListScopesFor, new { SourceId = id, Path = path });
        return scopes.Select(s => new Reference(s));
    }
}