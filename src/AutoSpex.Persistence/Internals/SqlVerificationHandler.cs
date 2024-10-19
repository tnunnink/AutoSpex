using System.Data;
using System.Text.Json;
using AutoSpex.Engine;
using Dapper;

namespace AutoSpex.Persistence;

public class SqlVerificationHandler : SqlMapper.TypeHandler<Verification>
{
    public override void SetValue(IDbDataParameter parameter, Verification? value)
    {
        parameter.Value = value is not null ? JsonSerializer.Serialize(value) : default;
    }

    public override Verification? Parse(object? value)
    {
        if (value is not string text)
            return default;

        return JsonSerializer.Deserialize<Verification>(text);
    }
}