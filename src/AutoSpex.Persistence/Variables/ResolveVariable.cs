using AutoSpex.Engine;
using FluentResults;

namespace AutoSpex.Persistence.Variables;

public record ResolveVariable(Guid NodeId, string Name) : IDbQuery<Result<Variable>>
{
    
}