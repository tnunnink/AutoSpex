using Dapper;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence.Settings;

[PublicAPI]
public record ReadSetting(string Key) : IRequest<string?>;

[UsedImplicitly]
internal class ReadSettingHandler(IConnectionManager manager) : IRequestHandler<ReadSetting, string?>
{
    private const string ReadSetting = "SELECT Value FROM Setting WHERE Key = @Key";

    public async Task<string?> Handle(ReadSetting request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var value = await connection.QuerySingleAsync<string?>(ReadSetting, new { request.Key });
        return value;
    }
}