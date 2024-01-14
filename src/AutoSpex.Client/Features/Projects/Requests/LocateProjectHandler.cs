using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Engine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features;

[UsedImplicitly]
public class LocateProjectHandler : IRequestHandler<ProjectRequest.LocateProject, Result>
{
    public Task<Result> Handle(ProjectRequest.LocateProject request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Try(() =>
        {
            var directory = request.Project.Directory;
            
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