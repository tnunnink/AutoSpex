using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence.Variables;

[PublicAPI]
public record LoadVariables(Guid NodeId) : IDbQuery<Result<IEnumerable<Variable>>>;

[UsedImplicitly]
internal class LoadVariablesHandler(IConnectionManager manager)
    : IRequestHandler<LoadVariables, Result<IEnumerable<Variable>>>
{
    private const string GetVariables = "SELECT VariableId, Name, Value FROM Variable WHERE NodeId = @NodeId";
    
    public async Task<Result<IEnumerable<Variable>>> Handle(LoadVariables request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var variables = await connection.QueryAsync<Variable>(GetVariables, new {request.NodeId});
        return Result.Ok(variables);
    }
}
