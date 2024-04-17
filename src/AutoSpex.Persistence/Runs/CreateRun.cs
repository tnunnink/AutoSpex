using AutoSpex.Engine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record CreateRun(Run Run) : IDbCommand<Result>;

internal class CreateRunHandler(IConnectionManager manager) : IRequestHandler<CreateRun, Result>
{
    private const string CreateRun =
        "INSERT INTO Run (RunId, RunnerId, SourceId, Result, Runner, Source, RanOn, RanBy, Verified, Passed, Failed, Errored, Duration, Average) " +
        "VALUES(@RunId, @RunnerId);";

    public Task<Result> Handle(CreateRun request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}