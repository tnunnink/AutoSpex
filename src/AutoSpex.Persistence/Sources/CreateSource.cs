using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record CreateSource(Source Source, Guid ParentId = default) : IDbCommand<Result>;

[UsedImplicitly]
internal class CreateSourceHandler(IConnectionManager manager) : IRequestHandler<CreateSource, Result>
{
    private const string CheckId =
        "SELECT COUNT() FROM Source WHERE SourceId = @SourceId";

    private const string GetParentId =
        "SELECT NodeId FROM Node WHERE ParentId is null and Type = 'Source'";

    private const string InsertNode =
        "INSERT INTO Node (NodeId, ParentId, Type, Name) VALUES (@SourceId, @ParentId, 'Source', @Name)";

    private const string InsertSource =
        """
        INSERT INTO Source (SourceId, TargetType, TargetName, ExportedBy, ExportedOn, Content)
        VALUES (@SourceId, @TargetType, @TargetName, @ExportedBy, @ExportedOn, @Content)
        """;

    public async Task<Result> Handle(CreateSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        using var transaction = connection.BeginTransaction();

        var exists = await connection.QuerySingleAsync<int>(CheckId, new { request.Source.SourceId, transaction });
        if (exists > 0)
            return Result.Fail($"Source already exists: {request.Source.SourceId}");

        var parent = await connection.QuerySingleAsync<Guid>(GetParentId, transaction);
        var parentId = request.ParentId != Guid.Empty ? request.ParentId : parent;

        var node = new
        {
            request.Source.SourceId,
            ParentId = parentId,
            request.Source.Name
        };

        await connection.ExecuteAsync(InsertNode, node, transaction);
        await connection.ExecuteAsync(InsertSource, request.Source, transaction);

        transaction.Commit();
        return Result.Ok();
    }
}