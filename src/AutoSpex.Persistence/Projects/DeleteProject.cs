using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

/// <summary>
/// This request will both remove the provided project from the local application database recent projects list and
/// attempt to delete the project file from disc.
/// </summary>
/// <param name="Project">The project to delete.</param>
[PublicAPI]
public record DeleteProject(Project Project) : ICommand<Result>;

[UsedImplicitly]
internal class DeleteProjectHandler(IConnectionManager manager)
    : IRequestHandler<DeleteProject, Result>
{
    private const string Command = "DELETE FROM Project WHERE Path = @Path";

    public async Task<Result> Handle(DeleteProject request, CancellationToken cancellationToken)
    {
        var project = request.Project;

        var delete = Result.Try(() => { File.Delete(project.Uri.LocalPath); },
            e => new Error("Could not delete project file. Ensure no active connections.").CausedBy(e));

        if (delete.IsFailed) return delete;
        
        var connection = await manager.Connect(Database.App, cancellationToken);
        await connection.ExecuteAsync(Command, new {Path = project.Uri.LocalPath});
        
        return Result.Ok();
    }
}