using System.Data;
using Dapper;

namespace AutoSpex.Persistence;

public class SqlUriHandler : SqlMapper.TypeHandler<Uri>
{
    public override void SetValue(IDbDataParameter parameter, Uri? uri)
    {
        if (uri is null)
        {
            parameter.Value = null;
            return;
        }

        parameter.Value = uri.LocalPath;
    }

    public override Uri? Parse(object? value)
    {
        if (value is not string path) return default;
        return Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out var uri) ? uri : default;
    }
}