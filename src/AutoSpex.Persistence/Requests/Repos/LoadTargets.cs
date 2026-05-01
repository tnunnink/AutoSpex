using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadTargets(Repo Repo) : IRequest<Result>;

[UsedImplicitly]
internal class LoadTargetsHandler(IConnectionManager manager) : IRequestHandler<LoadTargets, Result>
{
    private const string ListTargets = "SELECT Location FROM Target WHERE RepoId = @RepoId";
    
    public async Task<Result> Handle(LoadTargets request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        
        var targets = await connection.QueryAsync<string>(ListTargets, new { request.Repo.RepoId });
        
        //Clear any in memory targets to refresh the collection.
        request.Repo.ClearTargets();
        
        //Add each configured target.
        foreach (var target in targets)
        {
            request.Repo.AddTarget(target);
        }

        return Result.Ok();
    }
}