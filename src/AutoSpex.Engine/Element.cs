using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;
using Ardalis.SmartEnum;
using L5Sharp.Core;
using Module = L5Sharp.Core.Module;
using Task = L5Sharp.Core.Task;

// ReSharper disable ConvertToPrimaryConstructor

namespace AutoSpex.Engine;

/// <summary>
/// An enumeration of all usable Logix program element in which we can write specifications for. This smart enum class
/// adds functionality needed to this application to navigate the property graphic of the logix elements and components.
/// It also allows the ability to define some custom properties which expose getters to other data on the object as a property
/// which can be selected for various criteria. These types are persisted and deserialized by name using the
/// <see cref="SmartEnum{TEnum}"/> library.
/// </summary>
public abstract class Element : SmartEnum<Element, string>
{
    /// <summary>
    /// A dictionary of cached known or static properties for a given type. This is used to avoid always using reflection
    /// each time we want to get the list of properties for a type.
    /// </summary>
    private static readonly Lazy<ConcurrentDictionary<Type, List<Property>>> PropertyCache = new();

    /// <summary>
    /// Holds "custom" properties, or properties we attach to the element and provide a custom function to retrieve its value.
    /// This would allow derived classes to add properties or make method calls show up as a property as needed.
    /// This also allows us to support dynamic ExpandoObject objects that can have different properties at runtime. 
    /// </summary>
    private readonly List<Property> _customProperties = [];

    private Element(Type type) : base(type.Name, type.Name)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    private Element(string name, Type type) : base(name, type.FullName ?? throw new ArgumentNullException(nameof(type)))
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>
    /// The corresponding type this <see cref="Element"/> represents.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// The self-referencing property for this element.
    /// </summary>
    public Property This => new(nameof(This), Type, null, x => x, () => Properties);

    /// <summary>
    /// The collection of properties (both static and custom) that are defined for this element type.
    /// </summary>
    public IEnumerable<Property> Properties => GetProperties();

    /// <summary>
    /// Indicates that this is a component element (Tag, Program, etc.) that we want to make available for selection.
    /// </summary>
    public bool IsComponent => Type.IsAssignableTo(typeof(LogixComponent));

    /// <summary>
    /// The set of selectable elements that a query or spec can start with.
    /// </summary>
    public static IEnumerable<Element> Selectable => List.Where(e => e.IsComponent || e == Rung);

    public static readonly Element Default = new DefaultElement();
    public static readonly Element Controller = new ControllerElement();
    public static readonly Element DataType = new DataTypeElement();
    public static readonly Element AddOnInstruction = new AddOnInstructionElement();
    public static readonly Element Module = new ModuleElement();
    public static readonly Element Tag = new TagElement();
    public static readonly Element Program = new ProgramElement();
    public static readonly Element Routine = new RoutineElement();
    public static readonly Element Task = new TaskElement();
    public static readonly Element Rung = new RungElement();
    public static readonly Element Line = new LineElement();
    public static readonly Element Sheet = new SheetElement();
    public static readonly Element Block = new BlockElement();

    /// <summary>
    /// A <see cref="Element"/> that represents a dynamic object. This corresponds to the <see cref="ExpandoObject"/>
    /// for .NET. This object can have any set of custom properties. 
    /// </summary>
    /// <param name="expando">The <see cref="ExpandoObject"/> instance to use for registering custom properties.</param>
    /// <returns></returns>
    public static Element Dynamic(ExpandoObject? expando = null)
    {
        return new DynamicElement(expando);
    }

    /// <summary>
    /// Creates an instance of <see cref="Element"/> that represents a dynamic object with the provided origin and selections.
    /// </summary>
    /// <param name="origin">The origin property type from which the selections are made.</param>
    /// <param name="selections">The collection or selections that make up the properties of the dynamic object.</param>
    /// <returns>An instance of <see cref="Element"/> representing a dynamic object with custom selection properties.</returns>
    public static Element Dynamic(Property origin, IEnumerable<Selection> selections)
    {
        return new DynamicElement(selections, origin);
    }

    /// <summary>
    /// Tries to create an <see cref="Element"/> instance based on the provided <paramref name="type"/>.
    /// Returns a boolean indicating whether the creation was successful.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to use for creating the Element instance.</param>
    /// <param name="element">The resulting <see cref="Element"/> instance if creation was successful.</param>
    /// <returns>A boolean value indicating whether the Element was successfully created.</returns>
    public static bool TryFromType(Type type, out Element element)
    {
        if (type == typeof(ExpandoObject))
        {
            element = Dynamic();
            return true;
        }

        return TryFromName(type.Name, out element);
    }

    /// <summary>
    /// Retrieves an Element based on the provided Scope.
    /// </summary>
    /// <param name="scope">The Scope for which to retrieve the Element.</param>
    /// <returns>The Element corresponding to the provided Scope. If the Scope represents an Instruction, returns the AddOnInstruction Element; otherwise, retrieves the Element using the Scope's type.</returns>
    public static Element FromScope(Scope scope)
    {
        if (scope.Type == ScopeType.Instruction) return AddOnInstruction;
        return TryFromName(scope.Type, out var match) ? match : Default;
    }

    /// <summary>
    /// Retrieves a property from the current object based on the specified path.
    /// </summary>
    /// <param name="path">The path to the desired property, separated by dots.</param>
    /// <returns>The <see cref="GetProperty"/> object representing the specified property if found, otherwise, <c>null</c>.</returns>
    public Property GetProperty(string? path) => This.GetProperty(path);

    /// <summary>
    /// Retrieves key-value pairs of indexable properties and their respective values for a given L5X content.
    /// </summary>
    /// <param name="content">The <see cref="L5X"/> content to query for extracting properties and values.</param>
    /// <returns>A collection of key-value pairs, where the key is a <see cref="Property"/> and the value is the corresponding object value.</returns>
    public virtual IEnumerable<KeyValuePair<Property, object>> IndexValues(L5X content)
    {
        var elements = content.Query(Type);

        foreach (var element in elements)
        {
            foreach (var property in Properties.Where(p => p.Group.IsIndexable))
            {
                var value = property.GetValue(element);
                if (value is null) continue;
                yield return new KeyValuePair<Property, object>(property, value);
            }
        }
    }

    /// <summary>
    /// Retrieves and returns a list of properties for the current Element instance. This method
    /// first checks if the properties for the given Type are cached, and if not, it retrieves
    /// static properties of the Type using reflection. It excludes certain properties and adds
    /// any custom properties that have been defined for the Element instance.
    /// </summary>
    /// <returns>A list of properties associated with the Element instance.</returns>
    private List<Property> GetProperties()
    {
        if (Name == nameof(Dynamic))
        {
            return _customProperties;
        }

        //Get or cache static properties for this type.
        //Since they should not change at runtime, we can avoid reusing reflection every time.
        //Only perform this step for non-dynamic type elements.
        return PropertyCache.Value.GetOrAdd(Type, type =>
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0 && !p.Name.Contains("L5X"))
                .Select(x => new Property(x.Name, x.PropertyType, This))
                .Concat(_customProperties)
                .ToList();
        });
    }

    /// <summary>
    /// Registers a custom property for the element type using the provided property name and getter function.
    /// </summary>
    /// <param name="name">The name of the custom property.</param>
    /// <param name="getter">A function for retrieving the value of the property given an input object.</param>
    /// <typeparam name="T">The property return type.</typeparam>
    /// <remarks>This allows me to turn methods into properties so that they are discoverable from the UI. This requires
    /// that the method takes no arguments, but is a convenient way to provide some additional properties without changing the base API.</remarks>
    private void Register<T>(string name, Func<object?, object?> getter)
    {
        var property = new Property(name, typeof(T), This, getter);
        _customProperties.Add(property);
    }

    private class DefaultElement : Element
    {
        public DefaultElement() : base("None", typeof(object))
        {
        }

        public override IEnumerable<KeyValuePair<Property, object>> IndexValues(L5X content) => [];
    }

    private class DynamicElement : Element
    {
        public DynamicElement(ExpandoObject? obj = null) : base("Dynamic", typeof(ExpandoObject))
        {
            if (obj is null) return;
            IDictionary<string, object?> dictionary = obj;

            foreach (var item in dictionary)
            {
                var type = item.Value?.GetType() ?? typeof(object);
                var custom = new Property(item.Key, type, This, x => GetValue(x, item.Key));
                _customProperties.Add(custom);
            }
        }

        public DynamicElement(IEnumerable<Selection> selections, Property origin) : base("Dynamic",
            typeof(ExpandoObject))
        {
            foreach (var selection in selections)
            {
                var type = origin.GetProperty(selection.Property).Type;
                var custom = new Property(selection.Alias, type, This, x => GetValue(x, selection.Alias));
                _customProperties.Add(custom);
            }
        }

        private static object? GetValue(object? input, string key)
        {
            if (input is not IDictionary<string, object?> dictionary)
                throw new InvalidOperationException("Invalid object type. Expecting dynamic object.");

            if (!dictionary.TryGetValue(key, out var value))
                throw new InvalidOperationException($"Property {key} does not exist for the current object.");

            return value;
        }

        public override IEnumerable<KeyValuePair<Property, object>> IndexValues(L5X content) => [];
    }

    private class ControllerElement : Element
    {
        public ControllerElement() : base(typeof(Controller))
        {
        }
    }

    private class DataTypeElement : Element
    {
        public DataTypeElement() : base(typeof(DataType))
        {
            Register<Controller>("Controller", x => ((DataType)x!).L5X?.Controller);
            Register<List<LogixComponent>>("Dependencies", x => ((DataType)x!).Dependencies().ToList());
            Register<List<CrossReference>>("References", x => ((DataType)x!).References().ToList());
        }
    }

    private class AddOnInstructionElement : Element
    {
        public AddOnInstructionElement() : base(typeof(AddOnInstruction))
        {
            Register<Controller>("Controller", x => (x as AddOnInstruction)?.L5X?.Controller);
            Register<List<LogixComponent>>("Dependencies", x => (x as AddOnInstruction)?.Dependencies().ToList());
            Register<List<CrossReference>>("References", x => (x as AddOnInstruction)?.References().ToList());
        }
    }

    private class ModuleElement : Element
    {
        public ModuleElement() : base(typeof(Module))
        {
            Register<Controller>("Controller", x => (x as Module)?.L5X?.Controller);
            Register<List<LogixComponent>>("Dependencies", x => (x as Module)?.Dependencies().ToList());
            Register<List<CrossReference>>("References", x => (x as Module)?.References().ToList());
        }
    }

    private class TagElement : Element
    {
        public TagElement() : base(typeof(Tag))
        {
            Register<Controller>("Controller", x => (x as Tag)?.L5X?.Controller);
            Register<List<LogixComponent>>("Dependencies", x => (x as Tag)?.Dependencies().ToList());
            Register<List<CrossReference>>("References", x => (x as Tag)?.References().ToList());
            Register<IList<Tag>>("Members", x => (x as Tag)?.Members().ToList() ?? []);
        }

        public override IEnumerable<KeyValuePair<Property, object>> IndexValues(L5X content)
        {
            //Want to get all nested tag members as well.
            var elements = content.Query<Tag>().SelectMany(t => t.Members());

            foreach (var element in elements)
            {
                foreach (var property in Properties.Where(p => p.Group.IsIndexable))
                {
                    var value = property.GetValue(element);
                    if (value is null) continue;
                    yield return new KeyValuePair<Property, object>(property, value);
                }
            }
        }
    }

    private class ProgramElement : Element
    {
        public ProgramElement() : base(typeof(Program))
        {
            Register<Controller>("Controller", x => (x as Program)?.L5X?.Controller);
            Register<List<LogixComponent>>("Dependencies", x => (x as Program)?.Dependencies().ToList());
            Register<List<CrossReference>>("References", x => (x as Program)?.References().ToList());
        }
    }

    private class RoutineElement : Element
    {
        public RoutineElement() : base(typeof(Routine))
        {
            Register<Controller>("Controller", x => (x as Routine)?.L5X?.Controller);
            Register<List<LogixComponent>>("Dependencies", x => (x as Routine)?.Dependencies().ToList());
            Register<List<CrossReference>>("References", x => (x as Routine)?.References().ToList());
        }
    }

    private class TaskElement : Element
    {
        public TaskElement() : base(typeof(Task))
        {
            Register<Controller>("Controller", x => (x as Task)?.L5X?.Controller);
            Register<List<LogixComponent>>("Dependencies", x => (x as Task)?.Dependencies().ToList());
            Register<List<CrossReference>>("References", x => (x as Task)?.References().ToList());
        }
    }

    private class RungElement : Element
    {
        public RungElement() : base(typeof(Rung))
        {
            Register<List<CrossReference>>("References", x => (x as Rung)?.References().ToList());
            Register<List<Instruction>>("Instructions", x => (x as Rung)?.Text.Instructions().ToList());
            Register<List<TagName>>("Tags", x => (x as Rung)?.Text.Tags().ToList());
        }
    }

    private class LineElement : Element
    {
        public LineElement() : base(typeof(Line))
        {
        }
    }

    private class SheetElement : Element
    {
        public SheetElement() : base(typeof(Sheet))
        {
        }
    }

    private class BlockElement : Element
    {
        public BlockElement() : base(typeof(Block))
        {
        }
    }
}