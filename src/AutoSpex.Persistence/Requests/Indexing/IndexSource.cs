using AutoSpex.Engine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record IndexSource(Source Source) : IRequest<Result>;

[UsedImplicitly]
public class IndexSourceHandler(IConnectionManager manager) : IRequestHandler<IndexSource, Result>
{
    private const string InsertSource =
        "INSERT INTO Source (Hash, Name, IndexedOn) VALUES (@Hash, @Name, @IndexedOn)";

    private const string InsertProperty =
        """
        INSERT INTO Property (Element, Name, Type) VALUES (@Element, @Name, @Type);
        SELECT last_insert_rowid()
        """;

    public async Task<Result> Handle(IndexSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);


        return Result.Ok();
    }
}