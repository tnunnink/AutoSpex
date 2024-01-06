using AutoSpex.Engine;
using L5Sharp.Core;

namespace AutoSpex.Persistence;

public static class Extensions
{
    public static Type ToType(this string typeName)
    {
        var nativeType = Type.GetType(typeName);
        if (nativeType is not null) return nativeType;

        var logixType = typeof(LogixElement).Assembly.GetType(typeName);
        if (logixType is not null) return logixType;

        var engineType = typeof(Criterion).Assembly.GetType(typeName);
        if (engineType is not null) return engineType;

        throw new NotSupportedException(
            $"The type name '{typeName}' does not represent a type that is supported.");
    }
}