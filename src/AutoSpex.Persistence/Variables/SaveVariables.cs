using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence.Variables;

[PublicAPI]
public record SaveVariables(IEnumerable<Variable> Variables) : IDbCommand<Result>;

[UsedImplicitly]
internal class SaveVariablesHandler(IConnectionManager manager) : IRequestHandler<SaveVariables, Result>
{
    private const string NodeCount = "SELECT count() as [Total] FROM Node WHERE NodeId IN @Ids";

    private const string UpsertVariable =
        """
        INSERT INTO Variable(VariableId, NodeId, Name, Type, Data, Description)
        VALUES (@VariableId, @NodeId, @Name, @Type, @Data, @Description)
        ON CONFLICT DO UPDATE
        SET NodeId = @NodeId, Name = @Name, Type = @Type, Data = @Data, Description = @Description;
        """;

    private const string DeleteOrphaned =
        "DELETE FROM Variable WHERE NodeId IN @NodeIds AND VariableId NOT IN @VariableIds";

    public async Task<Result> Handle(SaveVariables request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        var nodeIds = request.Variables.Select(v => v.NodeId.ToString()).Distinct().ToList();
        
        //First check that all nodes exists. If not return a failed result because this means our constraint will fail.
        var count = await connection.QuerySingleAsync<int>(NodeCount, new {Ids = nodeIds});
        if (nodeIds.Count != count)
            return Result.Fail("Not all variables have a valid or existing node reference.");

        //From here we are good to begin the update
        using var transaction = connection.BeginTransaction();

        //First add or update all variables provided.
        foreach (var variable in request.Variables)
            await connection.ExecuteAsync(UpsertVariable, variable, transaction);

        //Then remove any orphaned variables.
        var variableIds = request.Variables.Select(v => v.VariableId.ToString()).ToList();
        await connection.ExecuteAsync(DeleteOrphaned, new {NodeIds = nodeIds, VariableIds = variableIds}, transaction);

        transaction.Commit();
        return Result.Ok();
    }
}