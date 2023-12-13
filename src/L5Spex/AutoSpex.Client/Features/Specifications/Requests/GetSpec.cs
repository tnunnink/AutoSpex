using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Specifications;

[PublicAPI]
public record LoadSpecRequest(SpecificationViewModel Spec) : IRequest<Result>;

public class GetSpecHandler : IRequestHandler<LoadSpecRequest, Result>
{
    public async Task<Result> Handle(LoadSpecRequest request, CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);
        return Result.Ok();
    }
}