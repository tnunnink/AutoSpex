using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Specifications;

[PublicAPI]
public record GetSpecRequest(Guid NodeId) : IRequest<Result<Spec>>;

public class GetSpecHandler : IRequestHandler<GetSpecRequest, Result<Spec>>
{
    public Task<Result<Spec>> Handle(GetSpecRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}