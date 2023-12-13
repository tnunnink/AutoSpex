using System.Linq.Expressions;
using System.Reflection;
using Ardalis.SmartEnum;
using AutoSpex.Engine.Operations;
using L5Sharp.Core;
using Module = L5Sharp.Core.Module;
using Task = L5Sharp.Core.Task;

namespace AutoSpex.Engine;

public abstract class Element : SmartEnum<Element, string>
{
    /// <summary>
    /// Holds compiled property getter functions so we don't have to recreate them each time we need to get a property.
    /// This will improve the overall performance when we go to run many criterion for many specifications.
    /// These are cached as the are accessed. We can't be greedy and create them ahead of time because of the recursive nature
    /// of the type structures and these being static type, we could cause overflow exceptions. 
    /// </summary>
    private readonly Dictionary<string, Func<object, object?>> _cache = new();
    
    /// <summary>
    /// Holds "custom" properties, or properties we attach to the element and provide a custom function to retrieve
    /// it's value. This would allow derived classes to add properties or make method calls show up as a property. 
    /// </summary>
    private readonly Dictionary<Property, Func<object, object?>> _customProperties = new();

    private Element(Type type) : base(type.Name, type.Name)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    public Type Type { get; }

    public IEnumerable<Property> Properties => GetProperties();
    
    public Func<L5X, IEnumerable<object>> Query => file => file.Query(Type);

    #region ComponentTypes

    public static readonly Element Controller = new ControllerElement();
    public static readonly Element DataType = new DataTypeElement();
    public static readonly Element AddOnInstruction = new AddOnInstructionElement();
    public static readonly Element Module = new ModuleElement();
    public static readonly Element Tag = new TagElement();
    public static readonly Element Program = new ProgramElement();
    public static readonly Element Routine = new RoutineElement();
    public static readonly Element Task = new TaskElement();
    public static readonly Element Trend = new TrendElement();
    public static readonly Element WatchList = new WatchListElement();

    #endregion

    #region ElementTypes

    public static readonly Element Block = new BlockElement();
    public static readonly Element Communications = new CommunicationsElement();
    public static readonly Element Connection = new ConnectionElement();
    public static readonly Element DataTypeMember = new DataTypeMemberElement();
    public static readonly Element Line = new LineElement();
    public static readonly Element Parameter = new ParameterElement();
    public static readonly Element Pen = new PenElement();
    public static readonly Element ParameterConnection = new ParameterConnectionElement();
    public static readonly Element Port = new PortElement();
    public static readonly Element RedundancyInfo = new RedundancyInfoElement();
    public static readonly Element Rung = new RungElement();
    public static readonly Element SafetyInfo = new SafetyInfoElement();
    public static readonly Element Security = new SecurityElement();
    public static readonly Element Sheet = new SheetElement();
    public static readonly Element WatchTag = new WatchTagElement();

    #endregion

    /// <summary>
    /// Represents a criterion used for filtering data based on a property, operation, and arguments.
    /// </summary>
    /// <param name="property">The property to filter on.</param>
    /// <param name="operation">The operation to perform on the property.</param>
    /// <param name="arguments">The arguments required for the operation.</param>
    /// <returns>A new instance of the <see cref="Has"/> class.</returns>
    public Criterion Has(string property, Operation operation, params object[] arguments) =>
        new(this, property, operation, arguments);

    /// <summary>
    /// Retrieves the getter function based on the given path.
    /// </summary>
    /// <param name="path">The path to retrieve the getter function.</param>
    /// <returns>The getter function.</returns>
    public Func<object, object?> Getter(string path) => GenerateGetter(path);

    /// <summary
    public Property Property(string path)
    {
        Property? property = null;
        var properties = Properties.ToList();

        while (!string.IsNullOrEmpty(path) && properties.Any())
        {
            var dot = path.IndexOf('.');
            var member = dot >= 0 ? path[..dot] : path;
            property = properties.SingleOrDefault(p => p.Name == member);
            properties = property?.Properties.ToList() ?? Enumerable.Empty<Property>().ToList();
            path = dot >= 0 ? path[(dot + 1)..] : string.Empty;
        }

        return property ?? throw new InvalidOperationException($"No property '{path}' defined for element '{Type}'");
    }

    private void Register<T>(string name, Func<object, object?> getter)
    {
        var property = new Property(name, typeof(T));
        _customProperties.TryAdd(property, getter);
    }
    
    private IEnumerable<Property> GetProperties()
    {
        var results = new HashSet<Property>();
        
        var typeProperties = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0 && PropertyExclusions().All(e => e != p.Name));

        foreach (var typeProperty in typeProperties)
            results.Add(new Property(typeProperty));

        foreach (var customProperty in _customProperties)
            results.Add(customProperty.Key);

        return results;
    }

    private Func<object, object?> GenerateGetter(string property)
    {
        if (_cache.TryGetValue(property, out var cached)) return cached;

        var custom = _customProperties.FirstOrDefault(p => p.Key.Name == property).Value;
        if (custom is not null)
        {
            _cache.Add(property, custom);
            return custom;
        }
        
        var parameter = Expression.Parameter(typeof(object), "x");
        var converted = Expression.TypeAs(parameter, Type);
        var member = GetMember(converted, property);
        
        var getter = Expression.Lambda<Func<object, object?>>(member, parameter).Compile();
        
        _cache.Add(property, getter);
        return getter;
    }

    /// <summary>
    /// Recursively gets the member expression for the provided property name. This will also add null checks for
    /// each nested member to prevent null reference exceptions, and to allow null to be propagated through the
    /// member expression and returns to the operation's evaluation method.
    /// </summary>
    /// <param name="parameter">The current member access expression for the type.</param>
    /// <param name="property">The current property name to create member access to.</param>
    /// <returns>An <see cref="Expression{TDelegate}"/> that represents member access to a immediate or nested/complex
    /// member property or field, with corresponding conditional null checks for each member level.</returns>
    private static Expression GetMember(Expression parameter, string property)
    {
        if (!property.Contains('.'))
            return Expression.TypeAs(Expression.PropertyOrField(parameter, property), typeof(object));

        var index = property.IndexOf('.');
        var member = Expression.PropertyOrField(parameter, property[..index]);
        var notNull = Expression.NotEqual(member, Expression.Constant(null));
        return Expression.Condition(notNull, GetMember(member, property[(index + 1)..]), Expression.Constant(null),
            typeof(object));
    }
    
    private static IEnumerable<string> PropertyExclusions()
    {
        return new[] {"L5X", "L5XType", "IsAttached"};
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
            Register<IEnumerable<LogixComponent>>("Dependencies", x => ((DataType)x).Dependencies());
        }
    }

    private class AddOnInstructionElement : Element
    {
        public AddOnInstructionElement() : base(typeof(AddOnInstruction))
        {
        }
    }

    private class ModuleElement : Element
    {
        public ModuleElement() : base(typeof(Module))
        {
        }
    }

    private class TagElement : Element
    {
        public TagElement() : base(typeof(Tag))
        {
        }
    }

    private class ProgramElement : Element
    {
        public ProgramElement() : base(typeof(Program))
        {
        }
    }

    private class RoutineElement : Element
    {
        public RoutineElement() : base(typeof(Routine))
        {
        }
    }

    private class TaskElement : Element
    {
        public TaskElement() : base(typeof(Task))
        {
        }
    }

    private class TrendElement : Element
    {
        public TrendElement() : base(typeof(Trend))
        {
        }
    }

    private class WatchListElement : Element
    {
        public WatchListElement() : base(typeof(WatchList))
        {
        }
    }

    private class BlockElement : Element
    {
        public BlockElement() : base(typeof(Block))
        {
        }
    }

    private class CommunicationsElement : Element
    {
        public CommunicationsElement() : base(typeof(Communications))
        {
        }
    }

    private class ConnectionElement : Element
    {
        public ConnectionElement() : base(typeof(Connection))
        {
        }
    }

    private class DataTypeMemberElement : Element
    {
        public DataTypeMemberElement() : base(typeof(DataTypeMember))
        {
        }
    }

    private class LineElement : Element
    {
        public LineElement() : base(typeof(Line))
        {
        }
    }

    private class ParameterElement : Element
    {
        public ParameterElement() : base(typeof(Parameter))
        {
        }
    }

    private class ParameterConnectionElement : Element
    {
        public ParameterConnectionElement() : base(typeof(ParameterConnection))
        {
        }
    }

    private class PenElement : Element
    {
        public PenElement() : base(typeof(Pen))
        {
        }
    }

    private class PortElement : Element
    {
        public PortElement() : base(typeof(Port))
        {
        }
    }

    private class RedundancyInfoElement : Element
    {
        public RedundancyInfoElement() : base(typeof(RedundancyInfo))
        {
        }
    }

    private class RungElement : Element
    {
        public RungElement() : base(typeof(Rung))
        {
        }
    }

    private class SafetyInfoElement : Element
    {
        public SafetyInfoElement() : base(typeof(SafetyInfo))
        {
        }
    }

    private class SecurityElement : Element
    {
        public SecurityElement() : base(typeof(Security))
        {
        }
    }

    private class SheetElement : Element
    {
        public SheetElement() : base(typeof(Sheet))
        {
        }
    }

    private class WatchTagElement : Element
    {
        public WatchTagElement() : base(typeof(WatchTag))
        {
        }
    }
}