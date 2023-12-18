using System.Threading;
using Avalonia.Input.Platform;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Projects;

[PublicAPI]
public record CopyPathRequest(Project Project) : IRequest;

[UsedImplicitly]
public class CopyPathHandler : IRequestHandler<CopyPathRequest>
{
    private readonly IClipboard _clipboard;

    public CopyPathHandler(IClipboard clipboard)
    {
        _clipboard = clipboard;
    }
    
    public Task Handle(CopyPathRequest request, CancellationToken cancellationToken)
    {
        return _clipboard.SetTextAsync(request.Project.File.DirectoryName);
    }
}