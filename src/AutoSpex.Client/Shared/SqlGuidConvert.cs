﻿using System;
using System.Data;
using Dapper;

namespace AutoSpex.Client.Shared;

public class SqlGuidTypeHandler : SqlMapper.TypeHandler<Guid>
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
        if (value == null)
        {
            return Guid.Empty;
        }
        
        return Guid.Parse((string)value);
    }
}