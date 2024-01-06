using System.Threading;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Input.Platform;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Requests;

[PublicAPI]
public record CopyPathRequest(Project Project) : IRequest;

[UsedImplicitly]
public class CopyPathHandler(IClipboard clipboard) : IRequestHandler<CopyPathRequest>
{
    public Task Handle(CopyPathRequest request, CancellationToken cancellationToken)
    {
        return clipboard.SetTextAsync(request.Project.Directory);
    }
}