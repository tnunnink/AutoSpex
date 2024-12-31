using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using L5Sharp.Core;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ResolveReferences(IEnumerable<Spec> Specs) : IRequest<Result>;

[UsedImplicitly]
internal class ResolveReferencesHandler(IConnectionManager manager) : IRequestHandler<ResolveReferences, Result>
{
    private const string GetContentByTarget = "SELECT Content FROM Source WHERE IsTarget = 1";
    private const string GetContentByName = "SELECT Content FROM Source WHERE Name = @Name";

    public async Task<Result> Handle(ResolveReferences request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var references = request.Specs.SelectMany(s => s.GetAllReferences()).Where(r => r.IsSource).ToList();
        var groups = references.GroupBy(r => r.Scope.Controller);

        foreach (var group in groups)
        {
            var content = string.IsNullOrEmpty(group.Key)
                ? await connection.QuerySingleOrDefaultAsync<L5X>(GetContentByTarget)
                : await connection.QuerySingleOrDefaultAsync<L5X>(GetContentByName, new { Name = group.Key });

            foreach (var reference in group)
                reference.ResolveUsing(content);
        }

        return Result.Ok();
    }
}