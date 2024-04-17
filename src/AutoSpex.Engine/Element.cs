using Ardalis.SmartEnum;
using L5Sharp.Core;
using Module = L5Sharp.Core.Module;
using Task = L5Sharp.Core.Task;

// ReSharper disable ConvertToPrimaryConstructor

namespace AutoSpex.Engine;

/// <summary>
/// A enumeration of all usable Logix program element in which we can write specifications for. This smart enum class
/// add functionality needed to this application to navigate the property graphic of the logix elements and components.
/// It also allows the ability to define some custom properties which expose getters to other data on the object as a property
/// which can be selected for various criteria. These types are persisted and deserialized by name using the
/// <see cref="SmartEnum{TEnum}"/> library.
/// </summary>
public abstract class Element : SmartEnum<Element, string>
{
    /// <summary>
    /// The name of the self referential property for the elements.
    /// </summary>
    private const string ThisProperty = "This";
    
    /// <summary>
    /// Holds "custom" properties, or properties we attach to the element and provide a custom function to retrieve
    /// it's value. This would allow derived classes to add properties or make method calls show up as a property as needed. 
    /// </summary>
    private readonly List<Property> _customProperties = [];

    private Element(Type type) : base(type.Name, type.Name)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        RegisterThis();
    }

    private Element(string name, Type type) : base(name, type.FullName ?? throw new ArgumentNullException(nameof(type)))
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        RegisterThis();
    }

    public Type Type { get; }
    public IEnumerable<Property> Properties => Type.Properties();
    public IEnumerable<Property> CustomProperties => _customProperties;
    public Property This => _customProperties.Single(p => p.Name == ThisProperty);
    public virtual Func<L5X, IEnumerable<object>> Query => file => file.Query(Type);
    public Func<L5X, string, object?> Lookup => (file, name) => file.Find(new ComponentKey(Type.Name, name));
    public bool IsComponent => Type.IsAssignableTo(typeof(LogixComponent));
    protected virtual bool IsSelectable => IsComponent;
    public static IEnumerable<Element> Components => List.Where(e => e.IsComponent);
    public static IEnumerable<Element> Selectable => List.Where(e => e.IsSelectable);

    public static readonly Element Default = new DefaultElement();

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
    /// Retrieves a property from the current object based on the specified path.
    /// </summary>
    /// <param name="path">The path to the desired property, separated by dots.</param>
    /// <returns>The <see cref="Property"/> object representing the specified property if found, otherwise, <c>null</c>.</returns>
    public Property? Property(string? path) => Type.Property(path);

    /// <summary>
    /// Registers a custom property for the element type using the provided property name and getter function.
    /// </summary>
    /// <param name="name">The name of the custom property.</param>
    /// <param name="getter">A function for retrieving the value of the property given a input object.</param>
    /// <typeparam name="T">The property return type.</typeparam>
    /// <remarks>This allows me to turn methods into properties so that they are discoverable from the UI. This requires
    /// that the method takes no arguments, but is a convenient way to provide some additional properties without changing the base API.</remarks>
    private void Register<T>(string name, Func<object?, object?> getter)
    {
        var property = new Property(Type, name, typeof(T), getter);
        _customProperties.Add(property);
    }

    /// <summary>
    /// Registers a custom property called "This" as a reference to current instance of an object to the internal
    /// custom property collection. This allows all elements to reference themselves as a property for filtering or
    /// validation.
    /// </summary>
    private void RegisterThis()
    {
        var property = new Property(Type, ThisProperty, Type, x => x);
        _customProperties.Add(property);
    }

    private class DefaultElement : Element
    {
        public DefaultElement() : base("None", typeof(object))
        {
        }
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
            Register<IEnumerable<CrossReference>>("References", x => ((DataType) x!).References());
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
            Register<IEnumerable<Tag>>("Members", x => ((Tag) x!).Members());
        }

        public override Func<L5X, IEnumerable<object>> Query => 
            x => x.Query<Tag>().SelectMany(t => t.Members()).Where(t => !t.TagName.Member.All(char.IsDigit));
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

        protected override bool IsSelectable => true;
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

        protected override bool IsSelectable => true;
    }

    private class LineElement : Element
    {
        public LineElement() : base(typeof(Line))
        {
        }

        protected override bool IsSelectable => true;
    }

    private class ParameterElement : Element
    {
        public ParameterElement() : base(typeof(Parameter))
        {
        }

        protected override bool IsSelectable => true;
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

        protected override bool IsSelectable => true;
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