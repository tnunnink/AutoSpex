using Ardalis.SmartEnum;
using L5Sharp.Core;

namespace AutoSpex.Engine;

public class TypeGroup : SmartEnum<TypeGroup, int>
{
    private TypeGroup(string name, int value) : base(name, value)
    {
    }

    public static TypeGroup Default => new(nameof(Default), 0);

    public static TypeGroup Text => new(nameof(Text), 1);

    public static TypeGroup Number => new(nameof(Number), 2);

    public static TypeGroup Boolean => new(nameof(Boolean), 3);

    public static TypeGroup Date => new(nameof(Date), 4);

    public static TypeGroup Enum => new(nameof(Enum), 5);

    public static TypeGroup Collection => new(nameof(Collection), 6);

    public static TypeGroup Element => new(nameof(Element), 7);

    public static TypeGroup Criterion => new(nameof(Criterion), 8);

    public static TypeGroup Variable => new(nameof(Variable), 9);

    public static TypeGroup FromType(Type type)
    {
        if (IsBooleanType(type)) return Boolean;
        if (IsNumericType(type)) return Number;
        if (IsTextType(type)) return Text;
        if (IsDateType(type)) return Date;
        if (IsEnumType(type)) return Enum;
        if (IsCollectionType(type)) return Collection;
        if (IsElementType(type)) return Element;
        if (IsCriterionType(type)) return Criterion;
        if (IsVariableType(type)) return Variable;
        return Default;
    }

    private static bool IsBooleanType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type == typeof(bool) || type == typeof(BOOL);
    }

    private static bool IsNumericType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return NumericTypes.Contains(type);
    }

    private static bool IsTextType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return TextTypes.Contains(type);
    }

    private static bool IsDateType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type == typeof(DateTime);
    }

    private static bool IsEnumType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsEnum || type.IsAssignableTo(typeof(LogixEnum));
    }

    private static bool IsCollectionType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
               type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    private static bool IsElementType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return Engine.Element.List.Any(e => e.Type == type);
    }

    private static bool IsCriterionType(Type type)
    {
        return type == typeof(Criterion);
    }

    private static bool IsVariableType(Type type)
    {
        return type == typeof(Variable);
    }

    private static readonly HashSet<Type> NumericTypes =
    [
        typeof(byte), typeof(sbyte),
        typeof(short), typeof(ushort),
        typeof(int), typeof(uint),
        typeof(long), typeof(ulong),
        typeof(float), typeof(double), typeof(decimal),
        typeof(SINT), typeof(USINT),
        typeof(INT), typeof(UINT),
        typeof(DINT), typeof(UDINT),
        typeof(LINT), typeof(ULINT),
        typeof(REAL), typeof(LREAL),
        typeof(Watchdog),
        typeof(TaskPriority),
        typeof(ScanRate),
        typeof(LogixType)
    ];

    private static readonly HashSet<Type> TextTypes =
    [
        typeof(string),
        typeof(StringType),
        typeof(STRING),
        typeof(NeutralText),
        typeof(TagName),
        typeof(L5Sharp.Core.Argument),
        typeof(LogixType)
    ];
}