using System.Collections;
using System.Net;
using System.Xml;
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

    public abstract Type DefaultType { get; }
    public abstract object? DefaultValue { get; }
    protected abstract bool Belongs(Type type);
    public abstract bool TryParse(string text, out object? value);

    public static readonly TypeGroup Default = new DefaultTypeGroup();

    public static readonly TypeGroup Boolean = new BooleanTypeGroup();

    public static readonly TypeGroup Number = new NumberTypeGroup();

    public static readonly TypeGroup Text = new TextTypeGroup();

    public static readonly TypeGroup Date = new DateTypeGroup();

    public static readonly TypeGroup Enum = new EnumTypeGroup();

    public static readonly TypeGroup Collection = new CollectionTypeGroup();

    public static readonly TypeGroup Element = new ElementTypeGroup();

    public static readonly TypeGroup Criterion = new CriterionTypeGroup();

    public static readonly TypeGroup Variable = new VariableTypeGroup();


    private static readonly List<TypeGroup> Exclusions = [Default, Criterion, Variable, Collection];

    public static IEnumerable<TypeGroup> Selectable =>
        List.Where(t => Exclusions.All(e => e != t)).OrderBy(x => x.Value);

    public static TypeGroup FromType(Type? type)
    {
        if (type is null) return Default;
        if (Boolean.Belongs(type)) return Boolean;
        if (Number.Belongs(type)) return Number;
        if (Text.Belongs(type)) return Text;
        if (Date.Belongs(type)) return Date;
        if (Enum.Belongs(type)) return Enum;
        if (Collection.Belongs(type)) return Collection;
        if (Element.Belongs(type)) return Element;
        if (Criterion.Belongs(type)) return Criterion;
        if (Variable.Belongs(type)) return Variable;
        return Default;
    }

    private class DefaultTypeGroup() : TypeGroup(nameof(Default), 0)
    {
        public override Type DefaultType => typeof(object);
        public override object? DefaultValue => default;
        protected override bool Belongs(Type? type) => type is null;

        public override bool TryParse(string text, out object? value)
        {
            throw new NotImplementedException();
        }
    }

    private class BooleanTypeGroup() : TypeGroup(nameof(Boolean), 1)
    {
        public override Type DefaultType => typeof(bool);
        public override object DefaultValue => default(bool);

        protected override bool Belongs(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type == typeof(bool) || type == typeof(BOOL);
        }

        public override bool TryParse(string text, out object? value)
        {
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
            typeof(AtomicData),
            typeof(Dimensions),
            typeof(ProductType),
            typeof(Vendor)
        ];

        public override Type DefaultType => typeof(int);
        public override object DefaultValue => default(int);

        protected override bool Belongs(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return NumericTypes.Contains(type);
        }

        public override bool TryParse(string text, out object? value)
        {
            if (!Radix.TryInfer(text, out var radix))
            {
                value = null;
                return false;
            }

            value = radix.ParseValue(text);
            return true;
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
            typeof(Revision),
            typeof(L5Sharp.Core.Argument),
            typeof(IPAddress),
            typeof(ComponentKey)
        ];

        public override Type DefaultType => typeof(string);
        public override object DefaultValue => string.Empty;

        protected override bool Belongs(Type type)
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
        public override Type DefaultType => typeof(DateTime);
        public override object DefaultValue => DateTime.Today;

        protected override bool Belongs(Type type)
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
        public override Type DefaultType => typeof(LogixEnum);
        public override object? DefaultValue => default(LogixEnum);

        protected override bool Belongs(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type.IsEnum || type.IsAssignableTo(typeof(LogixEnum));
        }

        public override bool TryParse(string text, out object? value)
        {
            throw new NotImplementedException();
        }
    }

    private class CollectionTypeGroup() : TypeGroup(nameof(Collection), 6)
    {
        public override Type DefaultType => typeof(List<object>);
        public override object DefaultValue => new List<object>();

        protected override bool Belongs(Type type)
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
        public override Type DefaultType => typeof(LogixElement);
        public override object? DefaultValue => default(LogixElement);

        protected override bool Belongs(Type type)
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
            catch (XmlException)
            {
                value = null;
                return false;
            }
            catch (InvalidOperationException)
            {
                value = null;
                return false;
            }
        }
    }

    private class CriterionTypeGroup() : TypeGroup(nameof(Criterion), 8)
    {
        public override Type DefaultType => typeof(Criterion);
        public override object DefaultValue => new Criterion();
        protected override bool Belongs(Type type) => type == typeof(Criterion);

        public override bool TryParse(string text, out object? value)
        {
            value = null;
            return false;
        }
    }

    private class VariableTypeGroup() : TypeGroup(nameof(Variable), 9)
    {
        public override Type DefaultType => typeof(Variable);
        public override object DefaultValue => new Variable();
        protected override bool Belongs(Type type) => type == typeof(Variable);

        public override bool TryParse(string text, out object? value)
        {
            value = null;
            return false;
        }
    }
}