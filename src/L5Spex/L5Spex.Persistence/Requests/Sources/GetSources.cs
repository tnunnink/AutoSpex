using Dapper;
using JetBrains.Annotations;
using L5Spex.Persistence.Records;
using MediatR;

namespace L5Spex.Persistence.Requests.Sources;

[PublicAPI]
public static class GetSources
{
    public record Request : IRequest<IEnumerable<SourceRecord>>;
    
    public class Handler : IRequestHandler<Request, IEnumerable<SourceRecord>>
    {
        private readonly IConnectionProvider _connectionProvider;

        public Handler(IConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public async Task<IEnumerable<SourceRecord>> Handle(Request request, CancellationToken cancellationToken)
        {
            using var connection = _connectionProvider.Connect();
            return await connection.QueryAsync<SourceRecord>("SELECT * FROM Source ORDER BY SourcePath DESC");
        }
    }
}