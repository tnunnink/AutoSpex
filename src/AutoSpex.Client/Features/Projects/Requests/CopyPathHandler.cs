using System.Threading;
using Avalonia.Input.Platform;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features;

[UsedImplicitly]
internal class CopyPathHandler(IClipboard clipboard) : IRequestHandler<ProjectRequest.CopyPath>
{
    public Task Handle(ProjectRequest.CopyPath request, CancellationToken cancellationToken) =>
        clipboard.SetTextAsync(request.Project.Directory);
}