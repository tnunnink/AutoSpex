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
    /// Holds "custom" properties, or properties we attach to the element and provide a custom function to retrieve
    /// its value. This would allow derived classes to add properties or make method calls show up as a property as needed. 
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

    public Type Type { get; }
    public Property This => Engine.Property.This(Type);
    public IEnumerable<Property> Properties => This.Properties;
    public IEnumerable<Property> CustomProperties => _customProperties;
    public bool IsComponent => Type.IsAssignableTo(typeof(LogixComponent));
    public static IEnumerable<Element> Components => List.Where(e => e.IsComponent);
    public static IEnumerable<Element> Selectable => List.Where(e => e.IsComponent || e == Rung);

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
    public Property Property(string? path) => This.GetProperty(path);

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
        var property = new Property(name, typeof(T), This, getter);
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
            Register<List<DataType>>("DataTypes", x => ((Controller)x!).L5X?.DataTypes.ToList());
            Register<List<AddOnInstruction>>("AddOnInstructions", x => ((Controller)x!).L5X?.Instructions.ToList());
            Register<List<Module>>("Modules", x => ((Controller)x!).L5X?.Modules.ToList());
            Register<List<Tag>>("Tags", x => ((Controller)x!).L5X?.Tags.ToList());
            Register<List<Program>>("Programs", x => ((Controller)x!).L5X?.Programs.ToList());
            Register<List<Task>>("Tasks", x => ((Controller)x!).L5X?.Tasks.ToList());
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

    private class TrendElement : Element
    {
        public TrendElement() : base(typeof(Trend))
        {
            Register<Controller>("Controller", x => (x as Trend)?.L5X?.Controller);
        }
    }

    private class WatchListElement : Element
    {
        public WatchListElement() : base(typeof(WatchList))
        {
            Register<Controller>("Controller", x => (x as WatchList)?.L5X?.Controller);
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
            Register<List<CrossReference>>("References", x => (x as Rung)?.References().ToList());
            Register<IEnumerable<Instruction>>("Instructions", x => (x as Rung)?.Text.Instructions());
            Register<IEnumerable<TagName>>("Tags", x => (x as Rung)?.Text.Tags());
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