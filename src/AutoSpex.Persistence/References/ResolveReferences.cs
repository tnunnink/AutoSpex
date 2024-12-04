using AutoSpex.Engine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence.References;

[PublicAPI]
public record ResolveReferences(IEnumerable<Spec> Specs) : IDbCommand<Result>;

[UsedImplicitly]
internal class ResolveReferencesHandler(IConnectionManager manager) : IRequestHandler<ResolveReferences, Result>
{
    public async Task<Result> Handle(ResolveReferences request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var references = request.Specs.SelectMany(s => s.GetAllReferences());
        
        
        
        

        return Result.Ok();
    }
}