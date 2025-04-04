using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence.Settings;

[PublicAPI]
public record SaveSetting(string Key, string? Value) : IRequest<Result>;

[UsedImplicitly]
internal class SaveSettingHandler(IConnectionManager manager) : IRequestHandler<SaveSetting, Result>
{
    private const string SaveSetting =
        "INSERT INTO Setting (Key, Value) VALUES (@Key, @Value) ON CONFLICT (Key) DO UPDATE SET Value = @Value";

    public async Task<Result> Handle(SaveSetting request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        await connection.ExecuteAsync(SaveSetting, request);
        return Result.Ok();
    }
}