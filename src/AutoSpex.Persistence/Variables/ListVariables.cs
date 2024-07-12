using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListVariables : IDbQuery<Result<IEnumerable<Variable>>>;

[UsedImplicitly]
internal class ListVariablesHandler(IConnectionManager manager)
    : IRequestHandler<ListVariables, Result<IEnumerable<Variable>>>
{
    private const string ListVariables = "SELECT * FROM Variable ORDER BY Name";
        
    public async Task<Result<IEnumerable<Variable>>> Handle(ListVariables request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var variables = await connection.QueryAsync<Variable>(ListVariables);
        return Result.Ok(variables);
    }
}