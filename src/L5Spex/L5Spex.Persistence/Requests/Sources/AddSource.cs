using Dapper;
using JetBrains.Annotations;
using LanguageExt.Common;
using MediatR;

namespace L5Spex.Persistence.Requests.Sources;

[PublicAPI]
public static class AddSource
{
    private const string InsertSource =
        "INSERT INTO Source (SourceId, Path, Selected, Pinned, Modified) VALUES (@Id, @Path, @Selected, @Pinned, @Modified)";

    public record Request(string Path) : IRequest<Result<Request>>;

    public class Handler : IRequestHandler<Request, Result<Request>>
    {
        private readonly IConnectionProvider _connectionProvider;

        public Handler(IConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public async Task<Result<Request>> Handle(Request request, CancellationToken cancellationToken)
        {
            using var connection = _connectionProvider.Connect();

            var exists =
                await connection.ExecuteScalarAsync<bool>(
                    "SELECT EXISTS(SELECT 1 FROM Source WHERE Path = @Path)", new {request.Path});

            if (exists) return new Result<Request>();

            await connection.ExecuteAsync(InsertSource,
                new
                {
                    Id = Guid.NewGuid(), request.Path, Selected = false, Pinned = false, Modified = DateTime.Now
                });

            return new Result<Request>(request);
        }
    }
}