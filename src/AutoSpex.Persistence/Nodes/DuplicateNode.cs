using AutoSpex.Engine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DuplicateNode(Node Node) : IDbCommand<Result>;

internal class DuplicateNodeHandler(IConnectionManager manager) : IRequestHandler<DuplicateNode, Result>
{
    private const string Duplicate = "";
    
    
    public async Task<Result> Handle(DuplicateNode request, CancellationToken cancellationToken)
    {
        var connection = await manager.Connect(cancellationToken);
        var transaction = connection.BeginTransaction();
        
        

        transaction.Commit();
        return Result.Ok();
    }
}

