using System.Data;
using Dapper;

namespace AutoSpex.Persistence;

public class SqlGuidHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid guid)
    {
        if (guid == Guid.Empty)
        {
            parameter.Value = null;
            return;
        }
        
        parameter.Value = guid.ToString();
    }

    public override Guid Parse(object? value)
    {
        return value is null or DBNull ? Guid.Empty : Guid.Parse((string)value);
    }
}