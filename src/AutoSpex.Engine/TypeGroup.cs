using System.Collections;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Ardalis.SmartEnum;
using L5Sharp.Core;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace AutoSpex.Engine;

public abstract class TypeGroup : SmartEnum<TypeGroup, int>
{
    private string WriteError => $"The provided value does not belong to the {Name} type group.";

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

    /// <summary>
    /// Reads and constructs an object from the provided Utf8JsonReader using the specified JsonSerializerOptions.
    /// </summary>
    /// <param name="reader">The Utf8JsonReader used to read the JSON data.</param>
    /// <param name="options">The JsonSerializerOptions used during deserialization.</param>
    /// <returns>The deserialized object constructed from the JSON data.</returns>
    public virtual object? ReadData(ref Utf8JsonReader reader, JsonSerializerOptions options) => default;

    /// <summary>
    /// Writes the data from the specified object to a Utf8JsonWriter using the specified JsonSerializerOptions
    /// based on the object's type group.
    /// </summary>
    /// <param name="writer">The Utf8JsonWriter to write the data to.</param>
    /// <param name="value">The object containing the data to be written.</param>
    /// <param name="options">The JsonSerializerOptions to be used during writing.</param>
    public virtual void WriteData(Utf8JsonWriter writer, object? value, JsonSerializerOptions? options = default)
    {
        writer.WriteStartObject();
        writer.WriteString("Group", Name);
        writer.WriteString("Data", value?.ToString() ?? string.Empty);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Represents the default type group used in the application.
    /// </summary>
    public static readonly TypeGroup Default = new DefaultTypeGroup();

    /// <summary>
    /// Represents a type group that handles boolean values.
    /// </summary>
    public static readonly TypeGroup Boolean = new BooleanTypeGroup();

    /// <summary>
    /// Represents a type group that handles numeric values.
    /// </summary>
    public static readonly TypeGroup Number = new NumberTypeGroup();

    /// <summary>
    /// Represents a type group that handles text values.
    /// </summary>
    public static readonly TypeGroup Text = new TextTypeGroup();

    /// <summary>
    /// Represents a type group that handles date values.
    /// </summary>
    public static readonly TypeGroup Date = new DateTypeGroup();

    /// <summary>
    /// Represents a type group that handles enumeration values.
    /// </summary>
    public static readonly TypeGroup Enum = new EnumTypeGroup();

    /// <summary>
    /// Represents a type group that handles any <see cref="LogixElement"/> derivative.
    /// </summary>
    public static readonly TypeGroup Element = new ElementTypeGroup();

    /// <summary>
    /// Represents a type group that handles collections of items.
    /// </summary>
    public static readonly TypeGroup Collection = new CollectionTypeGroup();

    /// <summary>
    /// Represents a type group that handles <see cref="Engine.Criterion"/> values.
    /// </summary>
    public static readonly TypeGroup Criterion = new CriterionTypeGroup();

    /// <summary>
    /// Represents a type group that handles <see cref="Engine.Range"/> values.
    /// </summary>
    public static readonly TypeGroup Range = new RangeTypeGroup();

    public static IEnumerable<TypeGroup> Selectable => List.Where(t => t != Default).OrderBy(x => x.Value);

    /// <summary>
    /// Retrieves the corresponding <see cref="TypeGroup"/> based on the provided <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The type to determine the group for.</param>
    /// <returns>The corresponding <see cref="TypeGroup"/> based on the provided type.</returns>
    public static TypeGroup FromType(Type? type)
    {
        if (type is null) return Default;
        if (Boolean.AppliesTo(type)) return Boolean;
        if (Number.AppliesTo(type)) return Number;
        if (Text.AppliesTo(type)) return Text;
        if (Date.AppliesTo(type)) return Date;
        if (Enum.AppliesTo(type)) return Enum;
        if (Element.AppliesTo(type)) return Element;
        if (Collection.AppliesTo(type)) return Collection;
        if (Criterion.AppliesTo(type)) return Criterion;
        if (Range.AppliesTo(type)) return Range;
        return Default;
    }

    #region Internal

    private class DefaultTypeGroup() : TypeGroup(nameof(Default), 0)
    {
        protected override bool AppliesTo(Type? type) => type is null;

        public override bool TryParse(string text, out object? value)
        {
            value = null;
            return false;
        }

        public override void WriteData(Utf8JsonWriter writer, object? value, JsonSerializerOptions? options = default)
        {
            writer.WriteStartObject();
            writer.WriteString("Group", Name);
            writer.WriteNull("Data");
            writer.WriteEndObject();
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

        public override object ReadData(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            return reader.GetBoolean();
        }

        public override void WriteData(Utf8JsonWriter writer, object? value, JsonSerializerOptions? options = default)
        {
            var data = value is bool boolean ? boolean : throw new ArgumentException(WriteError, nameof(value));

            writer.WriteStartObject();
            writer.WriteString("Group", Name);
            writer.WriteBoolean("Data", data);
            writer.WriteEndObject();
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

        public override object? ReadData(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return !string.IsNullOrEmpty(value) && TryParse(value, out var number) ? number : default;
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
            typeof(Instruction),
            typeof(TagName),
            typeof(Scope),
            typeof(Argument),
            typeof(Address),
            typeof(IPAddress)
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

        public override object? ReadData(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            return reader.GetString();
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

        public override object ReadData(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            return reader.GetDateTime();
        }
    }

    private class EnumTypeGroup() : TypeGroup(nameof(Enum), 5)
    {
        protected override bool AppliesTo(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type.IsAssignableTo(typeof(LogixEnum));
        }

        public override bool TryParse(string text, out object? value)
        {
            //if we don't know the type the user input, all we can do is get the first matching name. 
            var match = LogixEnum.Options().SelectMany(e => e.Value).FirstOrDefault(x => x.Name == text);
            value = match;
            return value is not null;
        }

        public override object? ReadData(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var data = reader.GetString()?.Split(':') ?? [];
            if (data.Length != 2) return default;

            var type = data[0].ToType();
            if (type is null) return default;

            return data[1].Parse(type);
        }

        public override void WriteData(Utf8JsonWriter writer, object? value, JsonSerializerOptions? options = default)
        {
            var data = value is LogixEnum enumeration
                ? string.Concat(enumeration.GetType(), ':', enumeration.Name)
                : throw new ArgumentException(WriteError, nameof(value));

            writer.WriteStartObject();
            writer.WriteString("Group", Name);
            writer.WriteString("Data", data);
            writer.WriteEndObject();
        }
    }

    private class ElementTypeGroup() : TypeGroup(nameof(Element), 6)
    {
        protected override bool AppliesTo(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return !type.IsEnumerable() && type.IsAssignableTo(typeof(LogixElement));
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

        public override object ReadData(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var bytes = reader.GetBytesFromBase64();
            var xml = Encoding.UTF8.GetString(bytes);
            var element = XElement.Parse(xml);
            return element.Deserialize<LogixElement>();
        }

        public override void WriteData(Utf8JsonWriter writer, object? value, JsonSerializerOptions? options = default)
        {
            var data = value is LogixElement element
                ? Encoding.UTF8.GetBytes(element.Serialize().ToString())
                : throw new ArgumentException(WriteError, nameof(value));

            writer.WriteStartObject();
            writer.WriteString("Group", Name);
            writer.WriteBase64String("Data", data);
            writer.WriteEndObject();
        }
    }

    private class CollectionTypeGroup() : TypeGroup(nameof(Collection), 7)
    {
        protected override bool AppliesTo(Type type) => type.IsEnumerable();

        public override bool TryParse(string text, out object? value)
        {
            try
            {
                value = JsonSerializer.Deserialize<List<object>>(text);
                return true;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }

        public override object ReadData(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException(WriteError);

            var group = Default;
            var result = new List<object?>();

            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType != JsonTokenType.PropertyName) continue;

                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "Group":
                        group = FromName(reader.GetString());
                        break;
                    case "Data":
                        var value = group.ReadData(ref reader, options);
                        result.Add(value);
                        break;
                }
            }

            return result;
        }

        public override void WriteData(Utf8JsonWriter writer, object? value, JsonSerializerOptions? options = default)
        {
            var data = value is IEnumerable enumerable
                ? enumerable.Cast<object>()
                : throw new ArgumentException(WriteError, nameof(value));

            writer.WriteStartObject();
            writer.WriteString("Group", Name);
            writer.WritePropertyName("Data");
            writer.WriteStartArray();

            foreach (var item in data)
            {
                var group = FromType(item?.GetType());
                group.WriteData(writer, item, options);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }

    private class CriterionTypeGroup() : TypeGroup(nameof(Criterion), 8)
    {
        protected override bool AppliesTo(Type type) => type == typeof(Criterion);

        public override bool TryParse(string text, out object? value)
        {
            try
            {
                value = JsonSerializer.Deserialize<Criterion>(text);
                return true;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }

        public override object? ReadData(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<Criterion>(ref reader, options);
        }

        public override void WriteData(Utf8JsonWriter writer, object? value, JsonSerializerOptions? options = default)
        {
            var data = value is Criterion criterion
                ? JsonSerializer.Serialize(criterion, options)
                : throw new InvalidOperationException($"The provided value is not a {Name} type group value.");

            writer.WriteStartObject();
            writer.WriteString("Group", Name);
            writer.WritePropertyName("Data");
            writer.WriteRawValue(data);
            writer.WriteEndObject();
        }
    }

    private class RangeTypeGroup() : TypeGroup(nameof(Range), 9)
    {
        protected override bool AppliesTo(Type type) => type == typeof(Range);

        public override bool TryParse(string text, out object? value)
        {
            try
            {
                value = JsonSerializer.Deserialize<Range>(text);
                return true;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }

        public override object? ReadData(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<Range>(ref reader, options);
        }

        public override void WriteData(Utf8JsonWriter writer, object? value, JsonSerializerOptions? options = default)
        {
            var data = value is Range range
                ? JsonSerializer.Serialize(range, options)
                : throw new InvalidOperationException($"The provided value is not a {Name} type group value.");

            writer.WriteStartObject();
            writer.WriteString("Group", Name);
            writer.WritePropertyName("Data");
            writer.WriteRawValue(data);
            writer.WriteEndObject();
        }
    }

    private class ReferenceTypeGroup() : TypeGroup(nameof(Reference), 10)
    {
        protected override bool AppliesTo(Type type) => type == typeof(Reference);

        public override bool TryParse(string text, out object? value)
        {
            try
            {
                //todo
                value = string.Empty;
                return true;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }

        public override object? ReadData(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var bytes = reader.GetBytesFromBase64();
            var data = Encoding.UTF8.GetString(bytes);
            return TryParse(data, out var reference) ? reference : default;
        }

        public override void WriteData(Utf8JsonWriter writer, object? value, JsonSerializerOptions? options = default)
        {
            var data = value is Reference reference
                ? reference.ToString()
                : throw new InvalidOperationException($"The provided value is not a {Name} type group value.");

            writer.WriteStartObject();
            writer.WriteString("Group", Name);
            writer.WriteBase64String("Data", Encoding.UTF8.GetBytes(data));
            writer.WriteEndObject();
        }
    }

    #endregion
}