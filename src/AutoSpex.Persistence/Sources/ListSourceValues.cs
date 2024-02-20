using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListSourceValues(Guid SourceId) : IDbQuery<Result<IEnumerable<string>>>;

internal class ListSourceValuesHandler(IConnectionManager manager)
    : IRequestHandler<ListSourceValues, Result<IEnumerable<string>>>
{
    public Task<Result<IEnumerable<string>>> Handle(ListSourceValues request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}


