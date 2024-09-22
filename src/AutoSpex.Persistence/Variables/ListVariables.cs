using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListVariables : IRequest<IEnumerable<Variable>>;

[UsedImplicitly]
internal class ListVariablesHandler(IConnectionManager manager) : IRequestHandler<ListVariables, IEnumerable<Variable>>
{
    private const string ListVariables = "SELECT * FROM Variable ORDER BY Name";

    public async Task<IEnumerable<Variable>> Handle(ListVariables request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var variables = await connection.QueryAsync<Variable>(ListVariables);
        return variables;
    }
}