using System.Data;
using System.Text.Json;
using AutoSpex.Engine;
using Dapper;

namespace AutoSpex.Persistence;

public class SqlSpecHandler : SqlMapper.TypeHandler<Spec>
{
    public override void SetValue(IDbDataParameter parameter, Spec? value)
    {
        var options = GetOptions();
        var data = JsonSerializer.Serialize(value, options);
        parameter.Value = data;
    }

    public override Spec? Parse(object? value)
    {
        if (value is not string text) return default;
        var options = GetOptions();
        return JsonSerializer.Deserialize<Spec>(text, options);
    }

    private static JsonSerializerOptions GetOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonSpecConverter());
        options.Converters.Add(new JsonObjectConverter());
        return options;
    }
}