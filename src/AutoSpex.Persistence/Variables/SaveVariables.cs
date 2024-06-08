using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SaveVariables(Guid NodeId, IEnumerable<Variable> Variables) : IDbCommand<Result>, IDbLoggable
{
    public string Message => $"Saved {Variables.Count()} variables.";
}

[UsedImplicitly]
internal class SaveVariablesHandler(IConnectionManager manager) : IRequestHandler<SaveVariables, Result>
{
    private const string NodeCount = "SELECT count() as [Total] FROM Node WHERE NodeId = @NodeId";

    private const string UpsertVariable =
        """
        INSERT INTO Variable([VariableId], [NodeId], [Name], [Group], [Type], [Data], [Description])
        VALUES (@VariableId, @NodeId, @Name, @Group, @Type, @Data, @Description)
        ON CONFLICT DO UPDATE
        SET NodeId = @NodeId, Name = @Name, [Group] = @Group, Type = @Type, Data = @Data, Description = @Description;
        """;

    private const string DeleteOrphaned =
        "DELETE FROM Variable WHERE NodeId = @NodeId AND VariableId NOT IN @VariableIds";

    public async Task<Result> Handle(SaveVariables request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        //First check that the nodes exists. If not return a failed result because this means our constraint will fail.
        var count = await connection.QuerySingleAsync<int>(NodeCount, new { request.NodeId });
        if (count != 1) return Result.Fail($"Node not found: {request.NodeId}");

        //From here we are good to begin the update
        using var transaction = connection.BeginTransaction();

        //First add or update all variables provided.
        foreach (var variable in request.Variables)
        {
            var record = new
            {
                variable.VariableId, request.NodeId, variable.Name, variable.Group,
                variable.Type, variable.Data, variable.Description
            };
            await connection.ExecuteAsync(UpsertVariable, record, transaction);
        }

        //Then remove any orphaned variables.
        var variableIds = request.Variables.Select(v => v.VariableId.ToString()).ToList();
        await connection.ExecuteAsync(DeleteOrphaned, new { request.NodeId, VariableIds = variableIds }, transaction);

        transaction.Commit();
        return Result.Ok();
    }
}