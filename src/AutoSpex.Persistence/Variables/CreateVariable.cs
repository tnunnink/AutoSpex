using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record CreateVariable(Guid NodeId, Variable Variable) : IDbCommand<Result>;

[UsedImplicitly]
internal class CreateVariableHandler(IConnectionManager manager) : IRequestHandler<CreateVariable, Result>
{
    private const string InsertVariable =
        """
        INSERT INTO Variable ([VariableId], [NodeId], [Name], [Group], [Value])
        VALUES (@VariableId, @NodeId, @Name, @Group, @Value)
        """;

    public async Task<Result> Handle(CreateVariable request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var record = new
        {
            request.Variable.VariableId,
            request.NodeId,
            request.Variable.Name,
            request.Variable.Group,
            request.Variable.Value
        };

        await connection.ExecuteAsync(InsertVariable, record);
        return Result.Ok();
    }
}