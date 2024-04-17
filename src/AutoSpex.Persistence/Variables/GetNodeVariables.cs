using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence.Variables;

[PublicAPI]
public record GetNodeVariables(Guid NodeId) : IDbQuery<Result<IEnumerable<Variable>>>;

[UsedImplicitly]
internal class GetNodeVariablesHandler(IConnectionManager manager)
    : IRequestHandler<GetNodeVariables, Result<IEnumerable<Variable>>>
{
    private const string GetVariables =
        """
        SELECT VariableId, NodeId, Name, Type, Data, Description
        FROM Variable WHERE NodeId = @NodeId ORDER BY Name;
        """;

    public async Task<Result<IEnumerable<Variable>>> Handle(GetNodeVariables request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var variables = await connection.QueryAsync<Variable>(GetVariables, new {request.NodeId});
        return Result.Ok(variables);
    }
}