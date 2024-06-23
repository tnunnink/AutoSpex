using System.Data;
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
    private const string GetRun =
        """
        SELECT r.RunId, n.Name, r.Result, r.RanOn, r.RanBy
            FROM Node n
            JOIN Run r on n.NodeId = r.RunId
            WHERE NodeId = @RunId
        """;

    private const string GetSpecNodes =
        """
        WITH SpecNodes AS (SELECT NodeId, ParentId, Type, Name, 0 as [Distance]
                      FROM Node
                      WHERE NodeId IN (SELECT SpecId FROM Outcome WHERE RunId = @RunId)
                      UNION ALL
                      SELECT n.NodeId, n.ParentId, n.Type, n.Name, t.Distance + 1
                      FROM Node n
                               INNER JOIN SpecNodes t ON t.ParentId = n.NodeId)

        SELECT NodeId, ParentId, Type, Name FROM SpecNodes ORDER BY Distance DESC
        """;

    private const string GetSourceNodes =
        """
        WITH SpecNodes AS (SELECT NodeId, ParentId, Type, Name, 0 as [Distance]
                      FROM Node
                      WHERE NodeId IN (SELECT SourceId FROM Outcome WHERE RunId = @RunId)
                      UNION ALL
                      SELECT n.NodeId, n.ParentId, n.Type, n.Name, t.Distance + 1
                      FROM Node n
                               INNER JOIN SpecNodes t ON t.ParentId = n.NodeId)

        SELECT NodeId, ParentId, Type, Name FROM SpecNodes ORDER BY Distance DESC
        """;

    private const string GetOutcomes =
        """
        SELECT OutcomeId, Result, Duration, SpecId, SourceId, Evaluations
        FROM Outcome
        WHERE RunId = @RunId
        """;

    public async Task<Result<Run>> Handle(GetRun request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        var run = await connection.QuerySingleOrDefaultAsync<Run>(GetRun, new { request.RunId });
        if (run is null) return Result.Fail($"Run not found: '{request.RunId}'");

        var specNodes = await GetNodes(connection, GetSpecNodes, request.RunId);
        var sourceNodes = await GetNodes(connection, GetSourceNodes, request.RunId);

        var outcomes = await connection.QueryAsync<Outcome, Guid, Guid, string, Outcome>(GetOutcomes,
            (outcome, specId, sourceId, evaluations) =>
            {
                var spec = specNodes.GetValueOrDefault(specId);
                var source = sourceNodes.GetValueOrDefault(sourceId);
                
                return outcome
                    .ConfigureSpec(spec)
                    .ConfigureSource(source)
                    .ConfigureEvaluations(evaluations);
            },
            splitOn: "SpecId,SourceId,Evaluations",
            param: new { request.RunId });

        run.AddOutcomes(outcomes);

        //todo overrides?

        return Result.Ok(run);
    }

    private static async Task<Dictionary<Guid, Node>> GetNodes(IDbConnection connection, string query, Guid runId)
    {
        var lookup = new Dictionary<Guid, Node>();

        var nodes = await connection.QueryAsync<Node>(query, new { RunId = runId });

        foreach (var node in nodes)
        {
            lookup.TryAdd(node.NodeId, node);

            if (lookup.TryGetValue(node.ParentId, out var parent))
            {
                parent.AddNode(node);
            }
        }

        return lookup;
    }
}