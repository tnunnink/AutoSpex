using System.Data;
using System.Text.Json;
using AutoSpex.Engine;
using Dapper;

namespace AutoSpex.Persistence;

public class SqlObjectHandler : SqlMapper.TypeHandler<object>
{
    public override void SetValue(IDbDataParameter parameter, object? value)
    {
        var options = GetOptions();
        var data = JsonSerializer.Serialize(value, options);
        parameter.Value = data;
    }

    public override object? Parse(object? value)
    {
        if (value is not string json) return default;
        var options = GetOptions();
        return JsonSerializer.Deserialize<object>(json, options);
    }

    private static JsonSerializerOptions GetOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonObjectConverter());
        return options;
    }
}