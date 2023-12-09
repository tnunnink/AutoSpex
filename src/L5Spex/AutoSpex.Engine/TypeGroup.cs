using AgileObjects.NetStandardPolyfills;
using Ardalis.SmartEnum;
using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// Groups types into simple groups and map the supported operations to each group.
/// </summary>
public class TypeGroup : SmartEnum<TypeGroup, int>
{
    private TypeGroup(string name, int value) : base(name, value)
    {
    }

    /// <summary>
    /// Default type group, only supports EqualTo and NotEqualTo.
    /// </summary>
    public static TypeGroup Default => new(nameof(Default), -1);

    /// <summary>
    /// Supports all text related operations.
    /// </summary>
    public static TypeGroup Text => new(nameof(Text), 1);

    /// <summary>
    /// Supports all numeric related operations.
    /// </summary>
    public static TypeGroup Number => new(nameof(Number), 2);

    /// <summary>
    /// Supports boolean related operations.
    /// </summary>
    public static TypeGroup Boolean => new(nameof(Boolean), 4);

    /// <summary>
    /// Supports all date related operations.
    /// </summary>
    public static TypeGroup Date => new(nameof(Date), 8);

    /// <summary>
    /// Supports all date related operations.
    /// </summary>
    public static TypeGroup Enum => new(nameof(Enum), 16);

    public static TypeGroup FromType(Type type)
    {
        if (IsBooleanType(type)) return Boolean;
        if (IsNumericType(type)) return Number;
        if (IsTextType(type)) return Text;
        if (IsDateType(type)) return Date;
        return IsEnumType(type) ? Enum : Default;
    }

    private static bool IsBooleanType(Type? type)
    {
        return type is not null && (type == typeof(bool) || type == typeof(BOOL));
    }

    private static bool IsTextType(Type? type)
    {
        return type is not null && (type == typeof(string) || type == typeof(StringType));
    }

    private static bool IsDateType(Type? type)
    {
        return type is not null && type == typeof(DateTime);
    }

    private static bool IsNumericType(Type type)
    {
        type = type.IsNullableType() ? Nullable.GetUnderlyingType(type)! : type;
        return NumericTypes.Contains(type);
    }

    private static bool IsEnumType(Type? type)
    {
        while (type != null && type != typeof(object))
        {
            var current = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            if (current.IsEnum || current == typeof(LogixEnum<,>)) return true;
            type = type.BaseType!;
        }

        return false;
    }

    private static readonly HashSet<Type> NumericTypes = new()
    {
        typeof(byte), typeof(sbyte),
        typeof(short), typeof(ushort),
        typeof(int), typeof(uint),
        typeof(long), typeof(ulong),
        typeof(float), typeof(double), typeof(decimal),
        typeof(SINT), typeof(USINT),
        typeof(INT), typeof(UINT),
        typeof(DINT), typeof(UDINT),
        typeof(LINT), typeof(ULINT),
        typeof(REAL), typeof(LREAL)
    };
}