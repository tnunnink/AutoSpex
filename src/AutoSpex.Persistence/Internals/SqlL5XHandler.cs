using System.Data;
using AutoSpex.Engine;
using Dapper;
using L5Sharp.Core;

namespace AutoSpex.Persistence;

public class SqlL5XHandler : SqlMapper.TypeHandler<L5X>
{
    public override void SetValue(IDbDataParameter parameter, L5X? content)
    {
        if (content is null)
        {
            parameter.Value = null;
            return;
        }

        parameter.Value = content.Serialize().ToString().Compress();
    }

    public override L5X Parse(object? value)
    {
        var data = value?.ToString();

        if (string.IsNullOrEmpty(data))
            return L5X.Empty();

        return L5X.Parse(data.Decompress(), L5XOptions.Index);
    }
}