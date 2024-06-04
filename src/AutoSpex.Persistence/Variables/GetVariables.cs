using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

/// <summary>
/// Gets all variables defined for the provided set of node id's.
/// </summary>
/// <param name="Nodes">The node id's for which to retrieve variables.</param>
[PublicAPI]
public record GetVariables(IEnumerable<Guid> Nodes) : IDbQuery<Result<IEnumerable<Variable>>>;

[UsedImplicitly]
internal class GetVariablesHandler(IConnectionManager manager)
    : IRequestHandler<GetVariables, Result<IEnumerable<Variable>>>
{
    private const string GetVariables =
        """
        SELECT [VariableId], [NodeId], [Name], [Group], [Type], [Data], [Description]
        FROM Variable WHERE NodeId IN @Ids
        """;

    public async Task<Result<IEnumerable<Variable>>> Handle(GetVariables request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var ids = request.Nodes.Select(n => n.ToString()).ToList();
        var variables = await connection.QueryAsync<Variable>(GetVariables, new { Ids = ids });
        return Result.Ok(variables);
    }
}