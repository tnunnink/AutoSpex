using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Projects;

[PublicAPI]
public record LocateProjectRequest(Project Project) : IRequest<Result>;

[UsedImplicitly]
public class LocateProjectHandler : IRequestHandler<LocateProjectRequest, Result>
{
    public Task<Result> Handle(LocateProjectRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Try(() =>
        {
            var directory = request.Project.File.DirectoryName ??
                            throw new ArgumentException(
                                $"No directory name available for project {request.Project.Name}");
            
            if (OperatingSystem.IsWindows())
                Process.Start(new ProcessStartInfo { FileName = directory, UseShellExecute = true });
            
            if (OperatingSystem.IsLinux())
                Process.Start("xdg-open", directory);
            
            else if (OperatingSystem.IsMacOS())
                Process.Start("open", directory);
            
            else throw new NotImplementedException("File browser opening not implemented for this platform.");
        }));
    }
}