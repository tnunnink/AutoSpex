using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SaveRun(Run Run) : IDbCommand<Result>, IDbLoggable
{
    public Guid NodeId => Run.RunId;
    public string Message => $"Saved Run '{Run.Name}'";
}

[UsedImplicitly]
internal class SaveRunHandler(IConnectionManager manager) : IRequestHandler<SaveRun, Result>
{
    public async Task<Result> Handle(SaveRun request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        using var transaction = connection.BeginTransaction();

        //todo the node exists already. all we do here is update the run nodes and run variables

        //1. delete all run nodes with this run id.
        //2. insert all nodes configured on the run.
        //3. delete all run variable with this run id.
        //4. insert all overriden variables for this run.

        transaction.Commit();
        return Result.Ok();
    }
}