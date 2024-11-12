using System.Net;
using System.Xml.Linq;
using Ardalis.SmartEnum;
using L5Sharp.Core;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace AutoSpex.Engine;

public abstract class TypeGroup : SmartEnum<TypeGroup, int>
{
    private TypeGroup(string name, int value) : base(name, value)
    {
    }

    /// <summary>
    /// Determines if this <see cref="TypeGroup"/> applies to the specficied <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if this group applies to the secified type; otherwise, <c>false</c>.</returns>
    protected abstract bool AppliesTo(Type type);

    /// <summary>
    /// Tries to parse the specified text into an object of this TypeGroup.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="value">The parsed object if the parsing was successful, otherwise null.</param>
    /// <returns>True if the parsing was successful; otherwise, false.</returns>
    public abstract bool TryParse(string text, out object? value);

    public static readonly TypeGroup Default = new DefaultTypeGroup();

    public static readonly TypeGroup Boolean = new BooleanTypeGroup();

    public static readonly TypeGroup Number = new NumberTypeGroup();

    public static readonly TypeGroup Text = new TextTypeGroup();

    public static readonly TypeGroup Date = new DateTypeGroup();

    public static readonly TypeGroup Enum = new EnumTypeGroup();

    public static readonly TypeGroup Collection = new CollectionTypeGroup();

    public static readonly TypeGroup Element = new ElementTypeGroup();

    public static IEnumerable<TypeGroup> Selectable => List.Where(t => t != Default).OrderBy(x => x.Value);

    public static TypeGroup FromType(Type? type)
    {
        if (type is null) return Default;
        if (Boolean.AppliesTo(type)) return Boolean;
        if (Number.AppliesTo(type)) return Number;
        if (Text.AppliesTo(type)) return Text;
        if (Date.AppliesTo(type)) return Date;
        if (Enum.AppliesTo(type)) return Enum;
        if (Collection.AppliesTo(type)) return Collection;
        if (Element.AppliesTo(type)) return Element;
        return Default;
    }

    private class DefaultTypeGroup() : TypeGroup(nameof(Default), 0)
    {
        protected override bool AppliesTo(Type? type) => type is null;

        public override bool TryParse(string text, out object? value)
        {
            value = null;
            return false;
        }
    }

    private class BooleanTypeGroup() : TypeGroup(nameof(Boolean), 1)
    {
        protected override bool AppliesTo(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type == typeof(bool) || type == typeof(BOOL);
        }

        public override bool TryParse(string text, out object? value)
        {
            if (bool.TryParse(text, out var boolean))
            {
                value = boolean;
                return true;
            }

            value = BOOL.TryParse(text);
            return value is not null;
        }
    }

    private class NumberTypeGroup() : TypeGroup(nameof(Number), 2)
    {
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
            typeof(LogixData),
            typeof(AtomicData),
            typeof(Dimensions),
            typeof(Revision),
            typeof(ProductType),
            typeof(Vendor)
        ];


        protected override bool AppliesTo(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return NumericTypes.Contains(type);
        }

        /// <summary>
        /// For numbers, I will just try to use an int/double for everything.
        /// If not an int, then use the Radix which supports all primitive numeric types.
        /// If not and the element is parsable, try to parse as LogixData which can be a number too (AtomicData).
        /// </summary>
        public override bool TryParse(string text, out object? value)
        {
            if (int.TryParse(text, out var i))
            {
                value = i;
                return true;
            }

            if (double.TryParse(text, out var d))
            {
                value = d;
                return true;
            }

            if (Radix.TryInfer(text, out var radix))
            {
                value = radix.ParseValue(text);
                return true;
            }

            try
            {
                var element = XElement.Parse(text);
                value = element.Deserialize();
                return true;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }
    }

    private class TextTypeGroup() : TypeGroup(nameof(Text), 3)
    {
        private static readonly HashSet<Type> TextTypes =
        [
            typeof(string),
            typeof(StringData),
            typeof(STRING),
            typeof(NeutralText),
            typeof(TagName),
            typeof(Argument),
            typeof(IPAddress),
            typeof(Scope)
        ];


        protected override bool AppliesTo(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return TextTypes.Contains(type);
        }

        public override bool TryParse(string text, out object? value)
        {
            value = text;
            return true;
        }
    }

    private class DateTypeGroup() : TypeGroup(nameof(Date), 4)
    {
        protected override bool AppliesTo(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type == typeof(DateTime);
        }

        public override bool TryParse(string text, out object? value)
        {
            if (DateTime.TryParse(text, out var parsed))
            {
                value = parsed;
                return true;
            }

            value = null;
            return false;
        }
    }

    private class EnumTypeGroup() : TypeGroup(nameof(Enum), 5)
    {
        protected override bool AppliesTo(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type.IsEnum || type.IsAssignableTo(typeof(LogixEnum));
        }

        public override bool TryParse(string text, out object? value)
        {
            //if we don't know the type the user input, all we can do is get the first matching name. 
            var match = LogixEnum.Options().SelectMany(e => e.Value).FirstOrDefault(x => x.Name == text);
            value = match;
            return value is not null;
        }
    }

    private class CollectionTypeGroup() : TypeGroup(nameof(Collection), 6)
    {
        protected override bool AppliesTo(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                   type.GetInterfaces().Any(x =>
                       x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        public override bool TryParse(string text, out object? value)
        {
            value = null;
            return false;
        }
    }

    private class ElementTypeGroup() : TypeGroup(nameof(Element), 7)
    {
        protected override bool AppliesTo(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type.IsAssignableTo(typeof(LogixElement));
        }

        public override bool TryParse(string text, out object? value)
        {
            try
            {
                var element = XElement.Parse(text);
                value = element.Deserialize();
                return true;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }
    }
}