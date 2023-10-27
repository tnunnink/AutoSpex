using Dapper;
using JetBrains.Annotations;
using LanguageExt.Common;
using MediatR;

namespace L5Spex.Persistence.Requests.Sources;

[PublicAPI]
public static class AddSource
{
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
                    "SELECT EXISTS(SELECT 1 FROM Source WHERE SourcePath = @Path)", new {request.Path});
            
            if (exists) return new Result<Request>();
            
            await connection.ExecuteAsync(
                "INSERT INTO Source (SourceId, SourcePath, Selected) VALUES (@Id, @Path, @Selected)",
                new {Id = Guid.NewGuid(), request.Path, Selected = false});

            return new Result<Request>(request);
        }
    }
}