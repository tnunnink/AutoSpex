using Ardalis.SmartEnum;
using AutoSpex.Engine.Operations;
using L5Sharp.Core;
using Module = L5Sharp.Core.Module;
using Task = L5Sharp.Core.Task;

namespace AutoSpex.Engine;

public abstract class Element : SmartEnum<Element, string>
{
    /// <summary>
    /// Holds "custom" properties, or properties we attach to the element and provide a custom function to retrieve
    /// it's value. This would allow derived classes to add properties or make method calls show up as a property. 
    /// </summary>
    private readonly List<Property> _customProperties = new();

    private Element(Type type) : base(type.Name, type.Name)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    public Type Type { get; }

    public IEnumerable<Property> Properties => GetProperties();

    public Func<L5X, IEnumerable<object>> Query => file => file.Query(Type);

    public bool IsComponent => ComponentType.All().Any(t => t.Name == Type.L5XType());

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
    public static Criterion Has(string property, Operation operation, params Arg[] arguments)
    {
        return new Criterion(property, operation, arguments);
    }

    /// <summary>
    /// Retrieves a property from the current object based on the specified path.
    /// </summary>
    /// <param name="path">The path to the desired property, separated by dots.</param>
    /// <returns>The <see cref="Property"/> object representing the specified property if found, otherwise, <c>null</c>.</returns>
    public Property? Property(string? path) => Type.Property(path);

    private void Register<T>(string name, Func<object?, object?> getter)
    {
        var property = new Property(Type, name, typeof(T), getter);
        _customProperties.Add(property);
    }

    /// <summary>
    /// Retrieves a collection of properties from the object's type and custom properties.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="Property"/> objects representing the properties of the object.
    /// </returns>
    private IEnumerable<Property> GetProperties()
    {
        var results = new HashSet<Property>();

        foreach (var property in Type.Properties())
            results.Add(property);

        foreach (var property in _customProperties)
            results.Add(property);

        return results;
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
            Register<IEnumerable<LogixComponent>>("Dependencies", x => ((DataType) x!).Dependencies());
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