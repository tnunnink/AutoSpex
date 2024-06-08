using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetRun(Guid RunId) : IDbCommand<Result<Run>>;

[UsedImplicitly]
internal class GetRunHandler(IConnectionManager manager) : IRequestHandler<GetRun, Result<Run>>
{
    private const string GetRun = "SELECT NodeId as [RunId], Name FROM Node WHERE NodeId = @RunId";

    private const string GetNodes =
        """
        SELECT n.NodeId, n.ParentId, n.Type, n.Name
        FROM RunNode r
        JOIN Node n on n.NodeId = r.NodeId
        WHERE r.RunId = @RunId
        """;

    public async Task<Result<Run>> Handle(GetRun request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        var run = await connection.QuerySingleOrDefaultAsync<Run>(GetRun, new { request.RunId });
        if (run is null) return Result.Fail($"Run not found: '{request.RunId}'");
        
        var nodes = await connection.QueryAsync<Node>(GetNodes, new { request.RunId });
        run.AddNodes(nodes);
        
        //todo variables

        return Result.Ok(run);
    }
}