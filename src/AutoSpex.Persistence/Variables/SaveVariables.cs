using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence.Variables;

[PublicAPI]
public record SaveVariables(Guid NodeId, IEnumerable<Variable> Variables) : IDbCommand<Result>;

[UsedImplicitly]
internal class AddVariablesHandler(IConnectionManager manager) : IRequestHandler<SaveVariables, Result>
{
    private const string UpsertVariable =
        "INSERT INTO Variable(VariableId, NodeId, Name, Value) " +
        "VALUES (@VariableId, @NodeId, @Name, @Value) " +
        "ON CONFLICT DO UPDATE " +
        "SET NodeId = @NodeId, Name = @Name, Value = @Value;";

    private const string DeleteOrphanedVariables =
        "DELETE FROM Variable WHERE NodeId = @NodeId AND VariableId NOT IN @Ids";

    public async Task<Result> Handle(SaveVariables request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        using var transaction = connection.BeginTransaction();

        foreach (var variable in request.Variables)
        {
            var record = new {variable.VariableId, request.NodeId, variable.Name, variable.Value};
            await connection.ExecuteAsync(UpsertVariable, record, transaction);
        }

        var ids = request.Variables.Select(v => v.VariableId.ToString()).ToList();
        await connection.ExecuteAsync(DeleteOrphanedVariables, new {request.NodeId, Ids = ids}, transaction);

        transaction.Commit();
        return Result.Ok();
    }
}