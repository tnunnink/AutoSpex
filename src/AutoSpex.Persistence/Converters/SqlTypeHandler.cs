using System.Data;
using Dapper;

namespace AutoSpex.Persistence;

public class SqlTypeHandler: SqlMapper.TypeHandler<Type>
{
    public override void SetValue(IDbDataParameter parameter, Type? type)
    {
        if (type is null)
        {
            parameter.Value = null;
            return;
        }
        
        parameter.Value = type.FullName;
    }

    public override Type? Parse(object? value)
    {
        var typeName = value?.ToString();
        
        if (string.IsNullOrEmpty(typeName)) 
            return default;

        return typeName.ToType();
    }
}