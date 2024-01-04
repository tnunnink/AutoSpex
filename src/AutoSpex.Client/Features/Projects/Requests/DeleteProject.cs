using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Behaviors;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Avalonia.Controls.Notifications;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Projects;

[PublicAPI]
public record DeleteProjectRequest(Project Project) : INotifiableRequest<Result>
{
    public Notification? BuildNotification(Result result)
    {
        if (result.IsFailed)
        {
            return new Notification("Action Failed",
                "The project can not be deleted at this time. Ensure no open connection are active and try again.",
                NotificationType.Error);
        }
        
        if (result.IsSuccess)
        {
            return new Notification("Project Deleted",
                $"The project {Project.Name} has been successfully deleted.",
                NotificationType.Success);
        }

        return default;
    }
}

[UsedImplicitly]
public class DeleteProjectHandler : IRequestHandler<DeleteProjectRequest, Result>
{
    private const string Command = "DELETE FROM Project WHERE Path = @Path";

    private readonly AppDatabase _database;

    public DeleteProjectHandler(AppDatabase database)
    {
        _database = database;
    }

    public async Task<Result> Handle(DeleteProjectRequest request, CancellationToken cancellationToken)
    {
        //todo definitely prompt first. or do this from the view model?

        var project = request.Project;

        var connection = await _database.Connect(cancellationToken);
        await connection.ExecuteAsync(Command, new { Path = project.Uri.LocalPath });

        return Result.Try(() => { File.Delete(project.Uri.LocalPath); });
    }
}