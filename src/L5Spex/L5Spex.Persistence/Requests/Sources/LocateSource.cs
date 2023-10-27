using System.Diagnostics;
using MediatR;

namespace L5Spex.Persistence.Requests.Sources;

public static class LocateSource
{
    public record Request(string Path) : IRequest;
    
    public class Handler : IRequestHandler<Request>
    {
        public Task Handle(Request request, CancellationToken cancellationToken)
        {
            var info = new FileInfo(request.Path);
            if (!info.Exists) return Task.CompletedTask; //todo should this do something else?
            Process.Start("explorer.exe", info.DirectoryName!);
            return Task.CompletedTask;
        }
    }
}