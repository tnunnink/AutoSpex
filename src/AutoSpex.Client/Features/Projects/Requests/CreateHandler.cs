using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Persistence;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features;

[UsedImplicitly]
internal class CreateHandler(IMediator mediator) : IRequestHandler<ProjectRequest.Create, Result>
{
    public Task<Result> Handle(ProjectRequest.Create request, CancellationToken cancellationToken) =>
        mediator.Send(new CreateProject(request.Project), cancellationToken);
}